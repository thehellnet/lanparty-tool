using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
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

        private List<Paragraph> _logList = new List<Paragraph>();

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
            var color = Brushes.Gray;
            var weight = FontWeights.Normal;
            var style = FontStyles.Italic;

            if (level == Level.Error)
            {
                color = Brushes.Red;
                weight = FontWeights.Bold;
                style = FontStyles.Normal;
            }
            else if (level == Level.Warn)
            {
                color = Brushes.Orange;
                weight = FontWeights.Normal;
                style = FontStyles.Normal;
            }
            else if (level == Level.Info)
            {
                color = Brushes.Black;
                weight = FontWeights.Normal;
                style = FontStyles.Normal;
            }

            var paragraph = new Paragraph();
            paragraph.Inlines.Add(message.Trim());
            paragraph.FontFamily = new FontFamily("Courier New");
            paragraph.Margin = new Thickness(0);
            paragraph.Foreground = color;
            paragraph.FontWeight = weight;
            paragraph.FontStyle = style;
            _logList.Add(paragraph);

            while (_logList.Count > MaxLogLines)
            {
                _logList.RemoveAt(0);
            }

            Dispatcher.Invoke((MethodInvoker) delegate
            {
                var flowDoc = LogText.Document;
                flowDoc.Blocks.Clear();
                flowDoc.Blocks.AddRange(_logList);
                LogText.Document = flowDoc;
            });
        }
    }
}