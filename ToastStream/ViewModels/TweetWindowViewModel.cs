using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ToastStream.Models;
using Rhinemaidens;
using ToastStream.Helpers;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace ToastStream.ViewModels
{
    public class TweetWindowViewModel : ViewModel
    {
        /* コマンド、プロパティの定義にはそれぞれ 
         * 
         *  lvcom   : ViewModelCommand
         *  lvcomn  : ViewModelCommand(CanExecute無)
         *  llcom   : ListenerCommand(パラメータ有のコマンド)
         *  llcomn  : ListenerCommand(パラメータ有のコマンド・CanExecute無)
         *  lprop   : 変更通知プロパティ(.NET4.5ではlpropn)
         *  
         * を使用してください。
         * 
         * Modelが十分にリッチであるならコマンドにこだわる必要はありません。
         * View側のコードビハインドを使用しないMVVMパターンの実装を行う場合でも、ViewModelにメソッドを定義し、
         * LivetCallMethodActionなどから直接メソッドを呼び出してください。
         * 
         * ViewModelのコマンドを呼び出せるLivetのすべてのビヘイビア・トリガー・アクションは
         * 同様に直接ViewModelのメソッドを呼び出し可能です。
         */

        /* ViewModelからViewを操作したい場合は、View側のコードビハインド無で処理を行いたい場合は
         * Messengerプロパティからメッセージ(各種InteractionMessage)を発信する事を検討してください。
         */

        /* Modelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedEventListenerや
         * CollectionChangedEventListenerを使うと便利です。各種ListenerはViewModelに定義されている
         * CompositeDisposableプロパティ(LivetCompositeDisposable型)に格納しておく事でイベント解放を容易に行えます。
         * 
         * ReactiveExtensionsなどを併用する場合は、ReactiveExtensionsのCompositeDisposableを
         * ViewModelのCompositeDisposableプロパティに格納しておくのを推奨します。
         * 
         * LivetのWindowテンプレートではViewのウィンドウが閉じる際にDataContextDisposeActionが動作するようになっており、
         * ViewModelのDisposeが呼ばれCompositeDisposableプロパティに格納されたすべてのIDisposable型のインスタンスが解放されます。
         * 
         * ViewModelを使いまわしたい時などは、ViewからDataContextDisposeActionを取り除くか、発動のタイミングをずらす事で対応可能です。
         */

        /* UIDispatcherを操作する場合は、DispatcherHelperのメソッドを操作してください。
         * UIDispatcher自体はApp.xaml.csでインスタンスを確保してあります。
         * 
         * LivetのViewModelではプロパティ変更通知(RaisePropertyChanged)やDispatcherCollectionを使ったコレクション変更通知は
         * 自動的にUIDispatcher上での通知に変換されます。変更通知に際してUIDispatcherを操作する必要はありません。
         */

        Lorelei lorelei;

        #region TweetBody変更通知プロパティ
        private string _TweetBody;

        public string TweetBody
        {
            get
            { return _TweetBody; }
            set
            { 
                if (_TweetBody == value)
                    return;
                _TweetBody = value;
                TweetLength = 140 - _TweetBody.Length;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TweetImageFilePath変更通知プロパティ
        private string _TweetImageFilePath;

        public string TweetImageFilePath
        {
            get
            { return _TweetImageFilePath; }
            set
            { 
                if (_TweetImageFilePath == value)
                    return;
                _TweetImageFilePath = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TweetImagePreview変更通知プロパティ
        private BitmapFrame _TweetImagePreview;

        public BitmapFrame TweetImagePreview
        {
            get
            { return _TweetImagePreview; }
            set
            { 
                if (_TweetImagePreview == value)
                    return;
                _TweetImagePreview = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region TweetLength変更通知プロパティ
        private int _TweetLength;

        public int TweetLength
        {
            get
            { return _TweetLength; }
            set
            { 
                if (_TweetLength == value)
                    return;
                _TweetLength = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public void Initialize()
        {
            lorelei = new Lorelei(Settings.ConsumerKey, Settings.ConsumerSecret, Settings.AccessToken, Settings.AccessTokenSecret);
            TweetLength = 140;
        }

        #region OpenImageCommand
        private ListenerCommand<OpeningFileSelectionMessage> _OpenImageCommand;

        public ListenerCommand<OpeningFileSelectionMessage> OpenImageCommand
        {
            get
            {
                if (_OpenImageCommand == null)
                {
                    _OpenImageCommand = new ListenerCommand<OpeningFileSelectionMessage>(OpenImage);
                }
                return _OpenImageCommand;
            }
        }

        public void OpenImage(OpeningFileSelectionMessage parameter)
        {
            try
            {
                // キャンセルするとエラーで死ぬ
                if (String.IsNullOrEmpty(parameter.Response[0]) == false)
                {
                    TweetImageFilePath = parameter.Response[0];

                    var ms = new MemoryStream(File.ReadAllBytes(TweetImageFilePath));
                    TweetImagePreview = BitmapFrame.Create(ms);
                }
            }
            catch { } // 仕方ない
        }
        #endregion

        #region PostTweetCommand
        private ViewModelCommand _PostTweetCommand;

        public ViewModelCommand PostTweetCommand
        {
            get
            {
                if (_PostTweetCommand == null)
                {
                    _PostTweetCommand = new ViewModelCommand(PostTweet);
                }
                return _PostTweetCommand;
            }
        }

        public void PostTweet()
        {
            try
            {
                if (TweetImagePreview != null)
                {
                    PostTweetWithImage();
                }
                else
                {
                    if (String.IsNullOrEmpty(TweetBody) == false)
                    {
                        PostTweetBodyOnly();
                    }
                }
            }
            catch (TooLongTweetBodyException)
            {
                var mbp = new MessageBoxPack("ツイート本文は140文字までです。", "ツイート エラー");
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch (DuplicateTweetBodyException)
            {
                var mbp = new MessageBoxPack("前回のツイートと同じ文章のようです。", "ツイート エラー");
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch (UnauthorizedException)
            {
                var mbp = new MessageBoxPack("Twitterとの認証情報のやりとりに失敗しました。", "ツイート エラー");
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
            catch (TwitterServerNotWorkingWellException)
            {
                var mbp = new MessageBoxPack("Twitterから不明なエラーが返ってきました。", "ツイート エラー");
                MessageBoxHelper.AddMessageBoxQueue(mbp);
            }
        }

        private async void PostTweetBodyOnly()
        {
            await Task.Run(() => 
            {
                try
                {
                    lorelei.PostTweet(TweetBody);

                    TweetBody = "";
                    TweetImageFilePath = null;
                    TweetImagePreview = null;
                }
                catch (TooLongTweetBodyException)
                {
                    var mbp = new MessageBoxPack("ツイート本文は140文字までです。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (DuplicateTweetBodyException)
                {
                    var mbp = new MessageBoxPack("前回のツイートと同じ文章のようです。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (UnauthorizedException)
                {
                    var mbp = new MessageBoxPack("Twitterとの認証情報のやりとりに失敗しました。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (TwitterServerNotWorkingWellException)
                {
                    var mbp = new MessageBoxPack("Twitterから不明なエラーが返ってきました。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
            });
        }

        private async void PostTweetWithImage()
        {
            if (String.IsNullOrEmpty(TweetBody) == true)
            {
                TweetBody = "";
            }
            await Task.Run(() =>
            {
                try
                {
                    lorelei.PostTweetWithImage(TweetBody, TweetImageFilePath);

                    TweetBody = "";
                    TweetImageFilePath = null;
                    TweetImagePreview = null;
                }
                catch (TooLongTweetBodyException)
                {
                    var mbp = new MessageBoxPack("ツイート本文は140文字までです。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (DuplicateTweetBodyException)
                {
                    var mbp = new MessageBoxPack("前回のツイートと同じ文章のようです。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (UnauthorizedException)
                {
                    var mbp = new MessageBoxPack("Twitterとの認証情報のやりとりに失敗しました。\n画像の送信に時間がかかりすぎています。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
                catch (TwitterServerNotWorkingWellException)
                {
                    var mbp = new MessageBoxPack("Twitterから不明なエラーが返ってきました。", "ツイート エラー");
                    MessageBoxHelper.AddMessageBoxQueue(mbp);
                }
            });

        }
        #endregion
    }
}
