using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using ToastStream.Models;
using ToastStream.Helpers;
using Rhinemaidens;
using System.Diagnostics;
using System.Threading.Tasks;


namespace ToastStream.ViewModels
{
    public class ConfigWindowViewModel : ViewModel
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


        #region SelectedIndex変更通知プロパティ
        private string _SelectedIndex;

        public string SelectedIndex
        {
            get
            { return _SelectedIndex; }
            set
            { 
                if (_SelectedIndex == value)
                    return;
                _SelectedIndex = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region IsAuthed変更通知プロパティ
        private bool _IsAuthed;

        public bool IsAuthed
        {
            get
            { return !_IsAuthed; }
            set
            { 
                if (_IsAuthed == value)
                    return;
                _IsAuthed = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ConsumerKey変更通知プロパティ
        private string _ConsumerKey;

        public string ConsumerKey
        {
            get
            { return _ConsumerKey; }
            set
            {
                if (_ConsumerKey == value)
                    return;
                _ConsumerKey = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ConsumerSecret変更通知プロパティ
        private string _ConsumerSecret;

        public string ConsumerSecret
        {
            get
            { return _ConsumerSecret; }
            set
            {
                if (_ConsumerSecret == value)
                    return;
                _ConsumerSecret = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region PIN変更通知プロパティ
        private string _PIN;

        public string PIN
        {
            get
            { return _PIN; }
            set
            { 
                if (_PIN == value)
                    return;
                _PIN = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AuthProgress変更通知プロパティ
        private string _AuthProgress;

        public string AuthProgress
        {
            get
            { return _AuthProgress; }
            set
            { 
                if (_AuthProgress == value)
                    return;
                _AuthProgress = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region ReceiveAllReplies変更通知プロパティ
        private bool _ReceiveAllReplies;

        public bool ReceiveAllReplies
        {
            get
            { return _ReceiveAllReplies; }
            set
            { 
                if (_ReceiveAllReplies == value)
                    return;
                _ReceiveAllReplies = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AllowUpdateCheck変更通知プロパティ
        private bool _AllowUpdateCheck;

        public bool AllowUpdateCheck
        {
            get
            { return _AllowUpdateCheck; }
            set
            { 
                if (_AllowUpdateCheck == value)
                    return;
                _AllowUpdateCheck = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region AllowAutoUpdate変更通知プロパティ
        private bool _AllowAutoUpdate;

        public bool AllowAutoUpdate
        {
            get
            { return _AllowAutoUpdate; }
            set
            { 
                if (_AllowAutoUpdate == value)
                    return;
                _AllowAutoUpdate = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        #region Version変更通知プロパティ
        private string _Version;

        public string Version
        {
            get
            { return _Version; }
            set
            { 
                if (_Version == value)
                    return;
                _Version = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        private string DefaultConsumerKey { get { return "T8qKNQBpiE5q1DWTwt0whWvj8"; } }
        private string DefaultConsumerSecret { get { return "J01XKPWjIvRUfSUeGhWXEk9q1Mo3ZHNkJRVo2C6TXFfQQVfOiy"; } }
        private string AccessToken { get; set; }
        private string AccessTokenSecret { get; set; }

        Lorelei lorelei;

        public void Initialize()
        {
            Version = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();

            lorelei = new Lorelei();
            ReadSettings();

            if (AccessToken != null)
            {
                IsAuthed = true;
                AuthProgress = "認証済みです。";
            }
            else
            {
                IsAuthed = false;
                AuthProgress = "認証されていません。 ①のボタンをクリックして認証してください。";
            }
        }

        private void ReadSettings()
        {
            ConsumerKey = Settings.ConsumerKey;
            AccessToken = Settings.AccessToken;
            ReceiveAllReplies = Settings.ReceiveAllReplies;
            AllowUpdateCheck = Settings.AllowUpdateCheck;
            AllowAutoUpdate = Settings.AllowAutoUpdate;
        }

        private void SaveSettings()
        {
            if (ConsumerKey != Settings.ConsumerKey)
            {
                Settings.ConsumerKey = ConsumerKey;
                Settings.ConsumerSecret = ConsumerSecret;
            }
            if (AccessToken != Settings.AccessToken)
            {
                Settings.AccessToken = AccessToken;
                Settings.AccessTokenSecret = AccessTokenSecret;
            }

            Settings.AllowUpdateCheck = AllowUpdateCheck;
            Settings.AllowAutoUpdate = AllowAutoUpdate;
            Settings.ReceiveAllReplies = ReceiveAllReplies;
        }

        #region OpenAuthUrlCommand
        private ViewModelCommand _OpenAuthUrlCommand;

        public ViewModelCommand OpenAuthUrlCommand
        {
            get
            {
                if (_OpenAuthUrlCommand == null)
                {
                    _OpenAuthUrlCommand = new ViewModelCommand(OpenAuthUrl);
                }
                return _OpenAuthUrlCommand;
            }
        }

        public async void OpenAuthUrl()
        {
            if (ConsumerKey == null || ConsumerKey == "" || ConsumerSecret == null || ConsumerSecret == "")
            {
                ConsumerKey = DefaultConsumerKey;
                ConsumerSecret = DefaultConsumerSecret;
            }

            lorelei.Initialize(ConsumerKey, ConsumerSecret);

            string url = null;
            try
            {
                await Task.Run(() =>  lorelei.GetOAuthUrl(out url));
            }
            catch { }

            if (url != null)
            {
                Process.Start(url);
                AuthProgress = "ToastStreamのアクセスを許可して、PINを入力してください。";
            }
            else
            {
                AuthProgress = "認証URLを取得できませんでした。しばらく待ってから再度認証してみてください。";
            }

        }
        #endregion

        #region GetAccessTokenCommand
        private ViewModelCommand _GetAccessTokenCommand;

        public ViewModelCommand GetAccessTokenCommand
        {
            get
            {
                if (_GetAccessTokenCommand == null)
                {
                    _GetAccessTokenCommand = new ViewModelCommand(GetAccessToken);
                }
                return _GetAccessTokenCommand;
            }
        }

        public async void GetAccessToken()
        {
            if (ConsumerKey == null || ConsumerKey == "" || ConsumerSecret == null || ConsumerSecret == "")
            {
                ConsumerKey = DefaultConsumerKey;
                ConsumerSecret = DefaultConsumerSecret;
            }

            lorelei.Initialize(ConsumerKey, ConsumerSecret);

            string token = null;
            string tokenSecret = null;
            try
            {
                await Task.Run(() => lorelei.GetAccessToken(PIN, out token, out tokenSecret));
            }
            catch { }

            if (token != null)
            {
                AccessToken = token;
                AccessTokenSecret = tokenSecret;

                IsAuthed = true;
                AuthProgress = "認証されています。";
            }
            else
            {
                AuthProgress = "トークンを取得できませんでした。しばらく待ってから再度認証してみてください。";
            }
        }
        #endregion

        #region OKCommand
        private ViewModelCommand _OKCommand;

        public ViewModelCommand OKCommand
        {
            get
            {
                if (_OKCommand == null)
                {
                    _OKCommand = new ViewModelCommand(OK);
                }
                return _OKCommand;
            }
        }

        public void OK()
        {
            SaveSettings();
            Close();
        }
        #endregion

        #region CancelCommand
        private ViewModelCommand _CancelCommand;

        public ViewModelCommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new ViewModelCommand(Cancel);
                }
                return _CancelCommand;
            }
        }

        public void Cancel()
        {
            Close();
        }
        #endregion

        #region ApplyCommand
        private ViewModelCommand _ApplyCommand;

        public ViewModelCommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new ViewModelCommand(Apply);
                }
                return _ApplyCommand;
            }
        }

        public void Apply()
        {
            SaveSettings();
        }
        #endregion

        private void Close()
        {
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }
    }
}
