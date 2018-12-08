using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using log4net;
using log4net.Core;
using LanPartyTool.config;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;

namespace LanPartyTool.windows
{
    public partial class MainWindow : Window
    {
        private const int MaxLogLines = 100;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));

        private readonly Config _config = Config.GetInstance();

        private List<string> _logList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            GameExeText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("GameExe"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            ToolCfgText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("ToolCfg"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            ProfileCfgText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("ProfileCfg"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            LogEvent.OnLogEvent += NewLogEvent;
        }

        private void NewLogEvent(Level level, string message)
        {
            var color = "grey";
            var weight = "normal";
            var style = "italic";

            if (level == Level.Error)
            {
                color = "red";
                weight = "bold";
                style = "normal";
            }
            else if (level == Level.Warn)
            {
                color = "orange";
                weight = "normal";
                style = "normal";
            }
            else if (level == Level.Info)
            {
                color = "black";
                weight = "normal";
                style = "normal";
            }

            _logList.Add(
                $"<div style='color: {color}; font-weight: {weight}; font-style: {style};'>{message.Trim()}</div>");

            while (_logList.Count > MaxLogLines)
            {
                _logList.RemoveAt(0);
            }

            var webContent = @"<div style='font-family: monospace; font-size: 12px; font-style: normal; font-weight: normal;'>";
            webContent += string.Join("", _logList);
            webContent += @"</div>";

            Dispatcher.Invoke((MethodInvoker) delegate { LogWebBrowser.NavigateToString(webContent); });
        }
    }
}