using System;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ToastStream.Models
{
    /// <summary>
    ///  XMLに書き出すための動的クラス
    /// </summary>
    public class XMLSettings
    {
        public byte[] Hash;
        public string ConsumerKey;
        public string ConsumerSecret;
        public string AccessToken;
        public string AccessTokenSecret;

        public bool ReceiveAllReplies = false;

        public bool AllowUpdateCheck = true;
        public bool AllowAutoUpdate = true;
    }

    /// <summary>
    ///  設定を読み書きするクラス
    /// </summary>
    internal class Settings
    {
        # region Memory
        /// <summary>
        ///  実際の設定値はここに記憶される
        /// </summary>
        protected class _Settings
        {
            public static byte[] _Hash { get; set; }

            public static string _ConsumerKey{ get; set; }
            public static string _ConsumerSecret { get; set; }
            public static string _AccessToken { get; set; }
            public static string _AccessTokenSecret { get; set; }

            public static bool? _ReceiveAllReplies { get; set; }

            public static bool? _AllowUpdateCheck { get; set; }
            public static bool? _AllowAutoUpdate { get; set; }
        }
        #endregion

        #region Accessor

        /// <summary>
        ///  PCごとの簡易ハッシュ値
        /// </summary>
        public static byte[] Hash
        {
            get { return _Settings._Hash; }
            set { _Settings._Hash = value; }
        }

        public static string ConsumerKey
        {
            get { return _Settings._ConsumerKey; }
            set { _Settings._ConsumerKey = value; }
        }

        public static string ConsumerSecret
        {
            get
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._ConsumerSecret, seed);
            }
            set
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._ConsumerSecret = Crypt.Encrypt(value, seed);
            }
        }

        public static string AccessToken
        {
            get { return _Settings._AccessToken; }
            set { _Settings._AccessToken = value; }
        }

        public static string AccessTokenSecret
        {
            get
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);

                return Crypt.Decrypt(_Settings._AccessTokenSecret, seed);
            }
            set
            {
                var mn = Environment.MachineName;
                var un = Environment.UserName;
                var udn = Environment.UserDomainName;

                var seed = Crypt.CreateSeed(mn + un + udn);
                _Settings._AccessTokenSecret = Crypt.Encrypt(value, seed);
            }
        }

        public static bool ReceiveAllReplies 
        {
            get
            {
                if (_Settings._ReceiveAllReplies == null) { return false; }
                else { return (bool)_Settings._ReceiveAllReplies; }
            }
            set { _Settings._ReceiveAllReplies = value; }
        }

        public static bool AllowUpdateCheck
        {
            get 
            {
                if (_Settings._AllowUpdateCheck == null) { return true; }
                else { return (bool)_Settings._AllowUpdateCheck; }
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        public static bool AllowAutoUpdate
        {
            get
            {
                if (_Settings._AllowAutoUpdate == null) { return true; }
                else { return (bool)_Settings._AllowAutoUpdate; }
            }
            set { _Settings._AllowAutoUpdate = value; }
        }

        #endregion Accessor

        private static string FileName = "Settings.xml";

        /// <summary>
        ///  設定を読み込むのに必要なシード値を生成し、設定の読み込みを試行する
        /// </summary>
        /// <returns>設定を読み込めたかどうか</returns>
        public static bool Initialize()
        {
            var mn = Environment.MachineName;
            var un = Environment.UserName;
            var udn = Environment.UserDomainName;

            var rawHash = Crypt.CreateSeed(mn + un + udn);
            var hash = Crypt.CreateSeed(rawHash);

            if (File.Exists(FileName) == true)
            {
                ReadSettings();

                if (Settings.Hash.SequenceEqual(hash) == false)
                {
                    Settings.AccessToken = null;
                    Settings.AccessTokenSecret = "";

                    return false;
                }

                return true;
            }
            else
            {
                Settings.AccessToken = null;
                Settings.AccessTokenSecret = "";
                Settings.Hash = hash;

                return false;
            }
        }

        /// <summary>
        ///  ファイルから設定を読み込む
        /// </summary>
        private static void ReadSettings()
        {
            var xmls = new XMLSettings();
            var xs = new XmlSerializer(typeof(XMLSettings));
            using (var fs = new FileStream(FileName, FileMode.Open))
            {
                xmls = (XMLSettings)xs.Deserialize(fs);
                fs.Close();
            }

            _Settings._Hash = xmls.Hash;
            _Settings._ConsumerKey = xmls.ConsumerKey;
            _Settings._ConsumerSecret = xmls.ConsumerSecret;
            _Settings._AccessToken = xmls.AccessToken;
            _Settings._AccessTokenSecret = xmls.AccessTokenSecret;
            _Settings._ReceiveAllReplies = xmls.ReceiveAllReplies;
            _Settings._AllowUpdateCheck = xmls.AllowUpdateCheck;
            _Settings._AllowAutoUpdate = xmls.AllowAutoUpdate;
        }

        /// <summary>
        ///  ファイルへ設定を書き込む
        /// </summary>
        public static void WriteSettings()
        {
            var xmls = new XMLSettings();
            xmls.Hash = _Settings._Hash;
            xmls.ConsumerKey = _Settings._ConsumerKey;
            xmls.ConsumerSecret = _Settings._ConsumerSecret;
            xmls.AccessToken = _Settings._AccessToken;
            xmls.AccessTokenSecret = _Settings._AccessTokenSecret;
            xmls.ReceiveAllReplies = Settings.ReceiveAllReplies;
            xmls.AllowUpdateCheck = Settings.AllowUpdateCheck;
            xmls.AllowAutoUpdate = Settings.AllowAutoUpdate;

            var xs = new XmlSerializer(typeof(XMLSettings));
            using (var fs = new FileStream(FileName, FileMode.Create, FileAccess.Write))
            {
                xs.Serialize(fs, xmls);
            }
        }
    }
}