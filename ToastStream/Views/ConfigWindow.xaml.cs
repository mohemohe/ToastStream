using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ToastStream.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ConfigWindow : Window
    {
        public ConfigWindow()
        {
            InitializeComponent();
            
            APIkeyTextBox_TextChanged(null, null);
            APIsecretTextBox_TextChanged(null, null);
        }

        private void Hyperlink_Navigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        }

        private void APIkeyTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (APIkeyTextBox.Text != "")
            {
                APIkeyTextBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            else
            {
                APIkeyTextBox.Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            }
        }

        private void APIsecretTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (APIsecretTextBox.Text != "")
            {
                APIsecretTextBox.Background = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            }
            else
            {
                APIsecretTextBox.Background = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255));
            }
        }
    }
}
