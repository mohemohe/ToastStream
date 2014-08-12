using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

using OAuth;

namespace Rhinemaidens
{
    public class Lorelei : ILorelei
    {
        private readonly string requestTokenUrl = "https://api.twitter.com/oauth/request_token";
        private readonly string authUrl = "https://api.twitter.com/oauth/authorize";
        private readonly string accessTokenUrl = "https://api.twitter.com/oauth/access_token";
        private readonly string userStreamUrl = "https://userstream.twitter.com/1.1/user.json";

        public string consumerKey { get; private set; }
        public string consumerSecret { get; private set; }
        public string accessToken { get; private set; }
        public string accessTokenSecret { get; private set; }

        private string requestToken { get; set; }
        private string requestTokenSecret { get; set; }

        private bool IsStartedUserStream { get; set; }

        public Queue<TweetInfo> TweetInfoQueue = new Queue<TweetInfo>();

        /// <summary>
        /// ツイートに関する情報を格納します
        /// </summary>
        public class TweetInfo
        {
            public string id { get; set; }
            public DateTime date { get; set; }
            public string userId { get; set; }
            public string screenName { get; set; }
            public string name { get; set; }
            public string iconUrl { get; set; }
            public string body { get; set; }

            public bool IsRetweet { get; set; }
            public string OriginId { get; set; }
            public DateTime OriginDate { get; set; }
            public string OriginUserId { get; set; }
            public string OriginScreenName { get; set; }
            public string OriginName { get; set; }
            public string OriginIconUrl { get; set; }
            public string OriginBody { get; set; }
        }

        /// <summary>
        /// ユーザーに関する情報を格納します
        /// </summary>
        class UserInfo
        {
            public string id { get; set; }
            public string screenName { get; set; }
        }

        public enum ImageSize
        {
            Normal,
            Mini,
            Bigger,
            Original
        }

        public Lorelei() { }

        public Lorelei(string ConsumerKey, string ConsumerSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
        }

        public Lorelei(string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
            this.accessToken = AccessToken;
            this.accessTokenSecret = AccessTokenSecret;
        }

        public void Initialize() { }

        public void Initialize(string ConsumerKey, string ConsumerSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
        }

        public void Initialize(string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret)
        {
            this.consumerKey = ConsumerKey;
            this.consumerSecret = ConsumerSecret;
            this.accessToken = AccessToken;
            this.accessTokenSecret = AccessTokenSecret;
        }

        /// <summary>
        /// OAuthに必要なヘッダを生成します
        /// </summary>
        /// <param name="EncodedUrl">エンコード済みURL</param>
        /// <param name="Method">GET, POST, etc...</param>
        /// <param name="ExtString1">URLの直後に必要な追加シグネチャ</param>
        /// <param name="ExtString2">oauth_versionの直後に必要な追加シグネチャ</param>
        /// <returns>ヘッダ文字列</returns>
        public string BuildHeaderString(string EncodedUrl, string Method, string ExtString1, string ExtString2)
        {
            OAuthBase oauth = new OAuthBase();
            string timeStamp = oauth.GenerateTimeStamp();
            string nonce = oauth.GenerateNonce();

            string signatureBase = GenerateSignatureBase(EncodedUrl.Replace("%3a", "%3A").Replace("%2f", "%2F"), Method, timeStamp, nonce, ExtString1, ExtString2);

            string compositeKey = consumerSecret + "&" + accessTokenSecret;

            string signature = oauth.GenerateSignatureUsingHash(signatureBase, new HMACSHA1(Encoding.UTF8.GetBytes(compositeKey)));

            signature = HttpUtility.UrlEncode(signature);

            string HeaderString = "OAuth oauth_consumer_key=\"" + consumerKey + "\", oauth_nonce=\"" + nonce + "\", oauth_signature=\""
                + signature + "\", " + "oauth_signature_method=\"HMAC-SHA1\", oauth_timestamp=\"" + timeStamp + "\", oauth_token=\""
                + accessToken + "\", " + "oauth_version=\"1.0\"";

            return HeaderString;
        }

        private string GenerateSignatureBase(string EncodedUrl, string Method, string TimeStamp, string Nonce, string ExtString1, string ExtString2)
        {
            string signatureBase = "";
            string AND = Uri.EscapeDataString("&").ToString();
            string EQ = Uri.EscapeDataString("=").ToString();

            signatureBase += Method + "&" + EncodedUrl + "&";

            if (ExtString1 != "")
            {
                signatureBase += Uri.EscapeDataString(ExtString1) + AND;
            }

            signatureBase += "oauth_consumer_key" + EQ + consumerKey + AND + "oauth_nonce" + EQ + Nonce + AND
                + "oauth_signature_method" + EQ + "HMAC-SHA1" + AND + "oauth_timestamp" + EQ + TimeStamp + AND
                + "oauth_token" + EQ + accessToken + AND + "oauth_version" + EQ + "1.0";

            if (ExtString2 != "")
            {
                signatureBase += AND + Uri.EscapeDataString(ExtString2);
            }

            return signatureBase;
        }

        private bool GetRequestToken()
        {
            OAuthBase oauth = new OAuthBase();
            string timeStamp = oauth.GenerateTimeStamp();
            string nonce = oauth.GenerateNonce();
            string method = "GET";

            string normalizedUrl, normalizedReqParams;

            string signature = oauth.GenerateSignature(new Uri(requestTokenUrl), consumerKey, consumerSecret, null, null
                , method, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedReqParams);

            string tokenUrl = normalizedUrl + "?" + normalizedReqParams + "&oauth_signature=" + signature;

            WebClient wc = new WebClient();
            string res;
            try
            {
                Stream st = wc.OpenRead(tokenUrl);
                StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                res = sr.ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new UnauthorizedException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }

            Regex re = new Regex("oauth_token=(?<token>.*)&oauth_token_secret=(?<tokenSecret>.*)&oauth_callback_confirmed");
            Match m = re.Match(res);

            requestToken = m.Groups["token"].Value;
            requestTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// OAuthでこの連携アプリへのアクセス許可を取得するためのURLを生成します
        /// </summary>
        /// <param name="OAuthUrl">認証URL</param>
        public void GetOAuthUrl(out string OAuthUrl)
        {
            if (GetRequestToken() == true)
            {
                OAuthUrl = authUrl + "?oauth_token=" + requestToken + "&oauth_token_secret=" + requestTokenSecret;
            }
            else
            {
                OAuthUrl = null;
            }
        }

        private bool _GetAccessToken(string pin)
        {
            OAuthBase oauth = new OAuthBase();
            string timeStamp = oauth.GenerateTimeStamp();
            string nonce = oauth.GenerateNonce();
            string method = "GET";

            string normalizedUrl, normalizedReqParams;

            string signature = oauth.GenerateSignature(new Uri(requestTokenUrl), consumerKey, consumerSecret, requestToken, requestTokenSecret
                , method, timeStamp, nonce, OAuthBase.SignatureTypes.HMACSHA1, out normalizedUrl, out normalizedReqParams);

            string tokenUrl = accessTokenUrl + "?" + normalizedReqParams + "&oauth_signature=" + signature + "&oauth_verifier=" + pin;

            WebClient wc = new WebClient();
            string res;
            try
            {
                Stream st = wc.OpenRead(tokenUrl);
                StreamReader sr = new StreamReader(st, Encoding.GetEncoding("UTF-8"));
                res = sr.ReadToEnd();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }

            Regex re = new Regex("oauth_token=(?<token>.*)&oauth_token_secret=(?<tokenSecret>.*)&user_id");
            Match m = re.Match(res);

            accessToken = m.Groups["token"].Value;
            accessTokenSecret = m.Groups["tokenSecret"].Value;

            return true;
        }

        /// <summary>
        /// ユーザが入力したPINからアクセストークンを取得します
        /// </summary>
        /// <param name="pin">PIN</param>
        /// <param name="AccessToken">AccessToken</param>
        /// <param name="AccessTokenSecret">AccessTokenSecret</param>
        public void GetAccessToken(string pin, out string AccessToken, out string AccessTokenSecret)
        {
            if (_GetAccessToken(pin) == true)
            {
                AccessToken = accessToken;
                AccessTokenSecret = accessTokenSecret;
            }
            else
            {
                AccessToken = null;
                AccessTokenSecret = null;
            }
        }

        /// <summary>
        /// ツイートを投稿します
        /// </summary>
        /// <param name="Body">本文</param>
        public void PostTweet(string Body)
        {
            if (Body.Length > 140)
            {
                throw new TooLongTweetBodyException();
            }

            string Url = "https://api.twitter.com/1.1/statuses/update.json";

            try
            {
                string method = "POST";
                string headerString = BuildHeaderString(HttpUtility.UrlEncode(Url), method, "", "status=" + Uri.EscapeDataString(Body));
                byte[] SendBytes = Encoding.UTF8.GetBytes("status=" + Uri.EscapeDataString(Body));

                HttpWebRequest Request = (HttpWebRequest)WebRequest.Create(Url);
                Request.Method = method;
                Request.Headers.Add(HttpRequestHeader.Authorization, headerString);
                Request.ContentType = "application/x-www-form-urlencoded";
                Request.ContentLength = SendBytes.Length;
                Request.ServicePoint.Expect100Continue = false;

                Stream ReqStream = Request.GetRequestStream();
                ReqStream.Write(SendBytes, 0, SendBytes.Length);
                ReqStream.Close();

                HttpWebResponse Response = (HttpWebResponse)Request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError)
                {
                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Unauthorized)
                    {
                        throw new UnauthorizedException();
                    }

                    if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                    {
                        throw new DuplicateTweetBodyException();
                    }
                }

                throw new TwitterServerNotWorkingWellException();
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }
        }


        public void GetUserInfo(long userId)
        {
            //TODO: 
        }

        public void GetImage(string ImageUrl, ImageSize Size, out Bitmap Image)
        {
            string url;

            if (Size == ImageSize.Bigger)
            {
                url = ImageUrl.Replace("_normal", "_bigger");
            }
            else if (Size == ImageSize.Mini)
            {
                url = ImageUrl.Replace("_normal", "_mini");
            }
            else if (Size == ImageSize.Original)
            {
                url = ImageUrl.Replace("_normal", "");
            }
            else
            {
                url = ImageUrl;
            }

            var wc = new WebClient();
            byte[] data;
            try
            {
                data = wc.DownloadData(url);
            }
            catch
            {
                throw new TwitterServerNotWorkingWellException();
            }
            var st = new MemoryStream(data);

            Image = new Bitmap(st);
        }

        /// <summary>
        /// UserStreamに接続します
        /// </summary>
        /// <param name="IsGetAllReplies">フォロー外のリプライも取得する</param>
        public async void ConnectUserStream(bool IsGetAllReplies)
        {
            IsStartedUserStream = true;
            try
            {
                await Task.Run(() => GetUserStream(IsGetAllReplies));
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// UserStreamを切断します
        /// </summary>
        public void DisconnectUserStream()
        {
            IsStartedUserStream = false;
        }

        private void GetUserStream(bool IsGetAllReplies)
        {
            TweetInfo ti;
            while (IsStartedUserStream)
            {
                WebResponse Response = null;
                {
                    int i = 0;
                    do
                    {
                        string method = "GET";
                        string headerString;
                        string url;
                        if (IsGetAllReplies)
                        {
                            url = userStreamUrl + "?replies=all";
                            headerString = BuildHeaderString(HttpUtility.UrlEncode(userStreamUrl), method, "", "replies=all");
                        }
                        else
                        {
                            url = userStreamUrl;
                            headerString = BuildHeaderString(HttpUtility.UrlEncode(userStreamUrl), method, "", "");
                        }

                        HttpWebRequest Request = (HttpWebRequest)HttpWebRequest.Create(url);
                        Request.Method = method;
                        Request.Headers.Add(HttpRequestHeader.Authorization, headerString);
                        Request.Timeout = Timeout.Infinite;
                        Request.ServicePoint.Expect100Continue = false;

                        try
                        {
                            Response = Request.GetResponse();
                        }
                        catch (WebException e)
                        {
                            if (((HttpWebResponse)e.Response).StatusCode == HttpStatusCode.Forbidden)
                            {
                                Thread.Sleep(2000 + (1000 * i));
                                i++;
                                if (i == 5)
                                {
                                    break;

                                }
                            }
                        }

                    } while (Response == null);
                }
                StreamReader Stream = new StreamReader(Response.GetResponseStream());

                while (IsStartedUserStream == true)
                {
                    try
                    {
                        string Text = Stream.ReadLine();
                        if (Text != null && Text.Length > 0)
                        {
                            var JsonRoot = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(Text);
                            if (JsonRoot.ContainsKey("user") && JsonRoot.ContainsKey("text"))
                            {
                                ti = new TweetInfo();

                                object tweetUserObj;
                                object tweetIdObj;
                                object tweetUserIdObj;
                                object tweetNameObj;
                                object tweetScreenNameObj;
                                object tweetIconUrlObj;
                                object tweetBodyObj;

                                var tweetUserSB = new StringBuilder();
                                JsonRoot.TryGetValue("user", out tweetUserObj);
                                var jrs = new JavaScriptSerializer();
                                jrs.Serialize(tweetUserObj, tweetUserSB);
                                var JsonUser = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetUserSB.ToString(0, tweetUserSB.Length));

                                JsonRoot.TryGetValue("id", out tweetIdObj);
                                JsonUser.TryGetValue("id", out tweetUserIdObj);
                                JsonUser.TryGetValue("name", out tweetNameObj);
                                JsonUser.TryGetValue("screen_name", out tweetScreenNameObj);
                                JsonUser.TryGetValue("profile_image_url_https", out tweetIconUrlObj);
                                JsonRoot.TryGetValue("text", out tweetBodyObj);

                                ti.id = tweetIdObj.ToString();
                                ti.userId = tweetUserIdObj.ToString();
                                ti.screenName = tweetScreenNameObj.ToString();
                                ti.name = tweetNameObj.ToString();
                                ti.iconUrl = tweetIconUrlObj.ToString();
                                ti.body = tweetBodyObj.ToString();

                                object tweetRetweetedStatus;

                                JsonRoot.TryGetValue("retweeted_status", out tweetRetweetedStatus);
                                if (tweetRetweetedStatus != null)
                                {
                                    object retweetOriginUserObj;
                                    object retweetOriginIdObj;
                                    object retweetOriginUserIdObj;
                                    object retweetOriginNameObj;
                                    object retweetOriginScreenNameObj;
                                    object retweetOriginIconUrlObj;
                                    object retweetOriginBodyObj;

                                    var tweetRetweetedStatusSB = new StringBuilder();
                                    jrs.Serialize(tweetRetweetedStatus, tweetRetweetedStatusSB);
                                    var JsonRetweet = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetRetweetedStatusSB.ToString(0, tweetRetweetedStatusSB.Length));
                                    JsonRetweet.TryGetValue("user", out retweetOriginUserObj);
                                    var tweetRetweetedOriginUserSB = new StringBuilder();
                                    jrs.Serialize(retweetOriginUserObj, tweetRetweetedOriginUserSB);
                                    var JsonRetweetOriginUser = new JavaScriptSerializer().Deserialize<Dictionary<string, object>>(tweetRetweetedOriginUserSB.ToString(0, tweetRetweetedOriginUserSB.Length));

                                    JsonRetweet.TryGetValue("id", out retweetOriginIdObj);
                                    JsonRetweetOriginUser.TryGetValue("id", out retweetOriginUserIdObj);
                                    JsonRetweetOriginUser.TryGetValue("name", out retweetOriginNameObj);
                                    JsonRetweetOriginUser.TryGetValue("screen_name", out retweetOriginScreenNameObj);
                                    JsonRetweetOriginUser.TryGetValue("profile_image_url_https", out retweetOriginIconUrlObj);
                                    JsonRetweet.TryGetValue("text", out retweetOriginBodyObj);

                                    ti.IsRetweet = true;
                                    ti.OriginId = retweetOriginIdObj.ToString();
                                    ti.OriginUserId = retweetOriginUserIdObj.ToString();
                                    ti.OriginName = retweetOriginNameObj.ToString();
                                    ti.OriginScreenName = retweetOriginScreenNameObj.ToString();
                                    ti.OriginIconUrl = retweetOriginIconUrlObj.ToString();
                                    ti.OriginBody = retweetOriginBodyObj.ToString();
                                }
                                else
                                {
                                    ti.IsRetweet = false;
                                }

                                TweetInfoQueue.Enqueue(ti);
                            }
                        }
                        else
                        {
                            Thread.Sleep(100);
                        }
                    }
                    catch
                    {
                        break;
                    }
                }
                try
                {
                    Response.Close();
                }
                catch { }
            }

            if (IsStartedUserStream)
            {
                throw new DeadOrDisconnectedUserStreamException();
            }

        }

        /// <summary>
        /// 画像をリサイズします
        /// </summary>
        /// <param name="SourceImage">リサイズ元の画像</param>
        /// <param name="Width">幅</param>
        /// <param name="Height">高さ</param>
        /// <param name="ResizedImage">リサイズ後の画像</param>
        public void ResizeImage(Bitmap SourceImage, int Width, int Height, out Bitmap ResizedImage)
        {
            double zoom;

            if ((double)Width / (double)Height <= (double)SourceImage.Width / (double)SourceImage.Height)
            {
                zoom = (double)Width / (double)SourceImage.Width;
            }
            else
            {
                zoom = (double)Height / (double)SourceImage.Height;
            }

            ResizedImage = new Bitmap((int)(SourceImage.Width * zoom), (int)(SourceImage.Height * zoom));
            var g = Graphics.FromImage(ResizedImage);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(SourceImage, 0, 0, ResizedImage.Width, ResizedImage.Height);
        }

        public void GenerateRetweeterImage(Bitmap SourceImage, int SourceImageWidth, int SourceImageHeight, Bitmap SourceOriginImage, int SourceOriginImageWidth, int SourceOriginImageHeight, int OffsetX, int OffsetY, out Bitmap GeneratedImage)
        {
            double SourceOriginImageZoom;
            double SourceImageZoom;

            if ((double)SourceImageWidth / (double)SourceImageHeight <= (double)SourceImage.Width / (double)SourceImage.Height)
            {
                SourceImageZoom = (double)SourceImageWidth / (double)SourceImage.Width;
            }
            else
            {
                SourceImageZoom = (double)SourceImageHeight / (double)SourceImage.Height;
            }

            if ((double)SourceOriginImageWidth / (double)SourceOriginImageHeight <= (double)SourceOriginImage.Width / (double)SourceOriginImage.Height)
            {
                SourceOriginImageZoom = (double)SourceOriginImageWidth / (double)SourceOriginImage.Width;
            }
            else
            {
                SourceOriginImageZoom = (double)SourceOriginImageHeight / (double)SourceOriginImage.Height;
            }

            GeneratedImage = new Bitmap((int)(SourceImage.Width * SourceImageZoom), (int)(SourceImage.Height * SourceImageZoom));
            using (var g = Graphics.FromImage(GeneratedImage))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(SourceImage, 0, 0, GeneratedImage.Width, GeneratedImage.Height);
                g.DrawImage(SourceOriginImage, OffsetX, OffsetY, SourceOriginImageWidth, SourceOriginImageHeight);
            }
        }
    }
}
