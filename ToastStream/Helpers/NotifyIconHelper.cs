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
        private static TweetWindow tw;
        private static ConfigWindow cw;

        public static async void Initialize()
        {
            Settings.Initialize();

            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("ToastStream.Views.Resource.ToastStream.ico"));
            notifyIcon.Visible = true;

            notifyIcon.DoubleClick += (sender, e) => MainWindowOpen();

            var cms = new ContextMenuStrip();
            var tsmi1 = new ToolStripMenuItem("Tweet");
            var tsmi2 = new ToolStripMenuItem("Config");
            var tsmi3 = new ToolStripMenuItem("Exit");
            cms.Items.AddRange(new ToolStripMenuItem[] { tsmi1, tsmi2, tsmi3 });

            tsmi1.Click += (sender, e) => MainWindowOpen();
            tsmi2.Click += (sender, e) => ConfigWindowOpen(true);
            tsmi3.Click += (sender, e) => MainWindowExit();

            notifyIcon.ContextMenuStrip = cms;

            tw = new TweetWindow();
            tw.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;

            var desktop = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea;
            tw.Top = desktop.Height - tw.Height;
            tw.Left = desktop.Width - tw.Width;

            if (Settings.AccessToken == null)
            {
                ConfigWindowOpen(false);
            }

            await Task.Run(() => m = new Model());
        }

        public static void Dispose()
        {
            notifyIcon.Dispose();
        }

        private static void MainWindowOpen()
        {
            tw.Show();
            tw.Activate();
        }

        public static void MainWindowClose()
        {
            tw.Hide();
        }

        public static void MainWindowExit()
        {
            Settings.WriteSettings();
            tw.Exit();
        }

        public static void ConfigWindowOpen(bool SaveSettings)
        {
            if (cw == null)
            {
                cw = new ConfigWindow();
                cw.ShowDialog();

                cw = null;
                if (SaveSettings == true)
                {
                    Settings.WriteSettings();
                }
            }
            else
            {
                try
                {
                    cw.Activate();
                }
                catch { }
            }
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
