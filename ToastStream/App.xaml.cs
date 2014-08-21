﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

using Livet;
using System.Threading;
using ToastStream.Helpers;
using System.IO;
using System.Globalization;
using ToastStream.Models;

namespace ToastStream
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        private Mutex mutex = new Mutex(false, "ToastStream");

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if (mutex.WaitOne(0, false) == false)
            {
                MessageBoxHelper.AddMessageBoxQueue(new MessageBoxPack("多重起動はできません。", "ToastStream", MessageBoxImage.Error));
                mutex.Close();
                mutex = null;
                this.Shutdown();
            }

            DispatcherHelper.UIDispatcher = Dispatcher;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Settings.Initialize();

            if (String.IsNullOrEmpty(Settings.Language) == false)
            {
                SetCurrentCulture(Settings.Language);
            }
            else
            {
                SetCurrentCulture("");
            }
            
            NotifyIconHelper.Initialize();
        }

        private void SetCurrentCulture(string CultureName)
        { 
            Thread.CurrentThread.CurrentCulture = new CultureInfo(CultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(CultureName); 
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            NotifyIconHelper.Dispose();

            if (mutex != null)
            {
                mutex.ReleaseMutex();
                mutex.Close();
            }
        }

        //集約エラーハンドラ
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string messeage = "未知のエラーが発生しました。アプリケーションを終了します。\n\n";
            string extMesseage = null;

            try
            {
                Exception ex = (Exception)e.ExceptionObject;

                messeage = "未知のエラーが発生しました。アプリケーションを終了します。\n\nエラー内容:\n";

                try
                {
                    var location = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                    using (var sw = new StreamWriter(location + @"\log.txt"))
                    {
                        sw.WriteLine("Messeage:");
                        sw.WriteLine(ex.Message);
                        sw.WriteLine("");
                        sw.WriteLine("InnerException:");
                        sw.WriteLine(ex.InnerException);
                        sw.WriteLine("");
                        sw.WriteLine("Source:");
                        sw.WriteLine(ex.Source);
                        sw.WriteLine("");
                        sw.WriteLine("TargetSite:");
                        sw.WriteLine(ex.TargetSite);
                        sw.WriteLine("");
                        sw.WriteLine("");
                        sw.WriteLine("StackTrace:");
                        sw.WriteLine(ex.StackTrace);
                        sw.Close();
                    }

                    extMesseage = "\n\n\n実行フォルダに log.txt を生成しました。";
                }
                catch { }

                MessageBoxHelper.AddMessageBoxQueue(new MessageBoxPack(
                    messeage + ex.Message + extMesseage,
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            catch
            {
                MessageBoxHelper.AddMessageBoxQueue(new MessageBoxPack(
                    messeage + "不明なエラー",
                    "エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error));
            }
            finally
            {
                Application_Exit(null, null);
            }
        }
    }
}
