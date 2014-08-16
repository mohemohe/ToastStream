using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Livet;
using Rhinemaidens;
using Win8Toast;

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

            try
            {
                lorelei.ConnectUserStream(Settings.ReceiveAllReplies);
            }
            catch (DeadOrDisconnectedUserStreamException)
            {
                lorelei.ConnectUserStream(Settings.ReceiveAllReplies);
            }
            while (true)
            {
                try
                {
                    ti = lorelei.tweetInfoQueue.Dequeue();

                    System.Drawing.Bitmap img1, img2, outImg;
                    var tmpImg = "~tmpIcon";

                    if (ti.IsRetweet == true)
                    {
                        lorelei.GetImage(ti.OriginIconUrl, Lorelei.ImageSize.Original, out img1);
                        lorelei.GetImage(ti.iconUrl, Lorelei.ImageSize.Original, out img2);
                        lorelei.GenerateRetweeterImage(img1, 150, 150, img2, 50, 50, 100, 100, out outImg);
                        outImg.Save(tmpImg);

                        Toast.ToastToastImageAndText02("@" + ti.OriginScreenName + " / " + ti.OriginName, ti.OriginBody, tmpImg);
                    }
                    else
                    {
                        lorelei.GetImage(ti.iconUrl, Lorelei.ImageSize.Original, out img1);
                        lorelei.ResizeImage(img1, 150, 150, out outImg);
                        outImg.Save(tmpImg);

                        Toast.ToastToastImageAndText02("@" + ti.screenName + " / " + ti.name, ti.body, tmpImg);
                    }
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
