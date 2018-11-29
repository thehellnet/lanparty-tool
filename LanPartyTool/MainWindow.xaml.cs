using System;
using System.Collections.Generic;
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
using LanPartyTool.log;

namespace LanPartyTool
{
    public partial class MainWindow : Window
    {
        private Logger _logger = Logger.GetLogger(typeof(MainWindow));

        public MainWindow()
        {
            InitializeComponent();

            var logEvent = Logger.GetLogEvent();
            logEvent += LogEventHandler;
        }

        private void LogEventHandler(string text)
        {
            LogText.AppendText(text);
        }
    }
}