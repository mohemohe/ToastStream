using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Rhinemaidens;
using Win8Toast;
using System.Drawing;
using ToastStream.Helpers;

namespace ToastStream.Models
{
    public class Model : NotificationObject
    {
        /*
         * NotificationObjectはプロパティ変更通知の仕組みを実装したオブジェクトです。
         */
        private Lorelei lorelei;

        public Model()
        {
            Initialize();
            ToastStream();
        }

        private void Initialize()
        {
            Toast.APP_ID = "ToastStream";
            Toast.silent = true;
            Toast.ICON_LOCATION = System.Environment.CurrentDirectory + "\\ToastStream.ico";
            Toast.TryCreateShortcut();
            
            if (Settings.AccessToken != null)
            {
                lorelei = new Lorelei(Settings.ConsumerKey, Settings.ConsumerSecret, Settings.AccessToken, Settings.AccessTokenSecret);
            }
        }

        private void ToastStream()
        {
            Lorelei.TweetInfo ti;
            Bitmap img1, img2, outImg;
            var tmpImg = "~tmpIcon";

            try
            {
                lorelei.ConnectUserStream(Settings.ReceiveAllReplies);
            }
            catch (DeadOrDisconnectedUserStreamException)
            {
                lorelei.ConnectUserStream(Settings.ReceiveAllReplies);
            }

            var ico = new System.Drawing.Icon(System.Environment.CurrentDirectory + "\\ToastStream.ico", 256, 256);
            img1 = ico.ToBitmap();
            ico.Dispose();
            lorelei.ResizeImage(150, 150, img1, out outImg);
            outImg.Save(tmpImg);
            Toast.ToastToastImageAndText02("ToastStream", "UserStreamに接続しました", tmpImg);

            while (true)
            {
                try
                {
                    ti = lorelei.tweetInfoQueue.Dequeue();

                    

                    if (ti.IsRetweet == true)
                    {
                        lorelei.GetImage(ti.OriginIconUrl, Lorelei.ImageSize.Original, out img1);
                        lorelei.GetImage(ti.iconUrl, Lorelei.ImageSize.Original, out img2);
                        lorelei.GenerateRetweeterImage(150, 150, img1, 128, 128, img2, 64, 64, out outImg);
                        outImg.Save(tmpImg);

                        Toast.ToastToastImageAndText02("@" + ti.OriginScreenName + " / " + ti.OriginName, ti.OriginBody, tmpImg);
                    }
                    else
                    {
                        lorelei.GetImage(ti.iconUrl, Lorelei.ImageSize.Original, out img1);
                        lorelei.ResizeImage(150, 150, img1, out outImg);
                        outImg.Save(tmpImg);

                        Toast.ToastToastImageAndText02("@" + ti.screenName + " / " + ti.name, ti.body, tmpImg);
                    }

                    img1 = null;
                    img2 = null;
                    outImg = null;
                    ti = null;
                }
                catch (DeadOrDisconnectedUserStreamException)
                {
                    lorelei.ConnectUserStream(Settings.ReceiveAllReplies);
                }
                catch { }

                System.Threading.Thread.Sleep(2);
            }
        }
    }
}
