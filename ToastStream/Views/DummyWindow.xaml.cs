﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ToastStream.Views
{
    /// <summary>
    /// DummyWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class DummyWindow : Window
    {
        bool ExitState { get; set; }

        public DummyWindow()
        {
            InitializeComponent();
            ExitState = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = !ExitState;
        }

        public void Exit()
        {
            ExitState = true;
            this.Close();
        }
    }
}
