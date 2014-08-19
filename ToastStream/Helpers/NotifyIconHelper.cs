using ToastStream.Models;
using ToastStream.ViewModels;
using ToastStream.Views;
using Livet;
using Livet.EventListeners;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace ToastStream.Helpers
{
    public class NotifyIconHelper
    {
        private static NotifyIcon notifyIcon;
        private static Model m;
        private static DummyWindow dw;

        public static async void Initialize()
        {
            Settings.Initialize();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ToastStream.Views.Resource.ToastStream.ico"));
            notifyIcon.Visible = true;

            notifyIcon.DoubleClick += (sender, e) => TweetWindowOpen();

            var cms = new ContextMenuStrip();
            var tsmi1 = new ToolStripMenuItem("Tweet");
            var tsmi2 = new ToolStripMenuItem("Config");
            var tsmi3 = new ToolStripMenuItem("Exit");
            cms.Items.AddRange(new ToolStripMenuItem[] { tsmi1, tsmi2, tsmi3 });

            tsmi1.Click += (sender, e) => TweetWindowOpen();
            tsmi2.Click += (sender, e) => ConfigWindowOpen(true);
            tsmi3.Click += (sender, e) => DummyWindowExit();

            notifyIcon.ContextMenuStrip = cms;

            dw = new DummyWindow(); // ループの確保

            if (Settings.AccessToken == null)
            {
                ConfigWindowOpen(false);
            }

            if (Settings.AccessToken != null)
            {
                Settings.WriteSettings();
                await Task.Run(() => m = new Model());
            }
            else
            {
                DummyWindowExit();
            }
        }

        public static void Dispose()
        {
            notifyIcon.Dispose();
        }

        private static void TweetWindowOpen()
        {
            var tw = new TweetWindow();
            tw.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

            var desktop = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            tw.Top = desktop.Height - tw.Height;
            tw.Left = desktop.Width - tw.Width;

            tw.ShowDialog();

            tw = null;
        }

        public static void DummyWindowExit()
        {
            Settings.WriteSettings();

            dw.Exit();
        }

        public static void ConfigWindowOpen(bool SaveSettings)
        {
            var cw = new ConfigWindow();
            cw.ShowDialog();

            if (SaveSettings == true)
            {
                Settings.WriteSettings();
            }

            cw = null;
        }

        #region public static void ShowNotifyBaloon()

        public static void ShowNotifyBaloon(string title, string body)
        {
            if (notifyIcon.Visible == true && notifyIcon.Icon != null)
            {
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = body;

                notifyIcon.ShowBalloonTip(10000);
            }
        }

        public static void ShowNotifyBaloon(string title, string body, int timeout)
        {
            if (notifyIcon.Visible == true && notifyIcon.Icon != null)
            {
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = body;

                notifyIcon.ShowBalloonTip(timeout);
            }
        }

        public static void ShowNotifyBaloon(string title, string body, ToolTipIcon icon, int timeout)
        {
            if (notifyIcon.Visible == true && notifyIcon.Icon != null)
            {
                notifyIcon.BalloonTipTitle = title;
                notifyIcon.BalloonTipText = body;
                notifyIcon.BalloonTipIcon = icon;

                notifyIcon.ShowBalloonTip(timeout);
            }
        }

        #endregion public static void ShowNotifyBaloon()
    }
}
