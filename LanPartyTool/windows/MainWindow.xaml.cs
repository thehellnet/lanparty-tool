using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Markup;
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

        private readonly object _logListLock = new object();
        private readonly List<dynamic> _logList = new List<dynamic>();

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

            dynamic logItem = new ExpandoObject();
            logItem.color = color;
            logItem.weight = weight;
            logItem.style = style;
            logItem.message = message.Trim();

            _logList.Add(logItem);
            while (_logList.Count > MaxLogLines)
            {
                _logList.RemoveAt(0);
            }

            LogText.Dispatcher.Invoke(DispatcherPriority.Background, new Action(() =>
            {
                var paragraph = new Paragraph();
                foreach (var log in _logList)
                {
                    paragraph.Inlines.Add(new TextBlock
                    {
                        FontFamily = new FontFamily("Courier New"),
                        Margin = new Thickness(0),
                        Foreground = log.color,
                        FontWeight = log.weight,
                        FontStyle = log.style,
                        Text = log.message
                    });
                }

                LogText.Document = new FlowDocument(paragraph);
            }));
        }
    }
}