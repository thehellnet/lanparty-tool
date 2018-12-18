using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using log4net;
using log4net.Core;
using LanPartyTool.agent;
using LanPartyTool.config;
using LanPartyTool.utility;

namespace LanPartyTool.windows
{
    public partial class MainWindow : Window
    {
        private const int MaxLogLines = 100;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));

        private readonly Status _status = Status.GetInstance();
        private readonly Config _config = Config.GetInstance();

        private readonly List<dynamic> _logList = new List<dynamic>();

        public MainWindow()
        {
            InitializeComponent();

            Title = $"{Application.ResourceAssembly.GetName().Name} {Application.ResourceAssembly.GetName().Version}";

            GameExeText.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath("GameExe"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            ToolCfgText.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath("ToolCfg"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            ProfileCfgText.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath("ProfileCfg"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            ServerAddressText.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath("ServerAddress"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            LogEvent.OnLogEvent += NewLogEvent;
            _status.OnSocketStatusChanged += SocketStatusChangedHandler;
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
                    paragraph.Inlines.Add(new LineBreak());
                }

                var document = new FlowDocument(paragraph);

                LogText.Document = document;
                LogText.ScrollToEnd();
            }));
        }

        private void SocketStatusChangedHandler()
        {
            SocketStatusText.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    switch (_status.SocketStatus)
                    {
                        case ServerSocket.Status.Closed:
                            SocketStatusText.Text = "Closed";
                            break;
                        case ServerSocket.Status.Preparing:
                            SocketStatusText.Text = "Preparing";
                            break;
                        case ServerSocket.Status.Listening:
                            SocketStatusText.Text = "Listening";
                            break;
                        case ServerSocket.Status.Accepting:
                            SocketStatusText.Text = "Accepting";
                            break;
                        case ServerSocket.Status.Closing:
                            SocketStatusText.Text = "Closing";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }));
        }

        private void GameExeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsUtility.OpenFileFolder(_config.GameExe);
        }

        private void ToolCfgButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsUtility.OpenFileFolder(_config.ToolCfg);
        }

        private void ProfileCfgButton_Click(object sender, RoutedEventArgs e)
        {
            WindowsUtility.OpenFileFolder(_config.ProfileCfg);
        }

        private void ShowCodKeyButton_Click(object sender, RoutedEventArgs e)
        {
            var codKey = GameUtility.ReadCodKey();
            var message = $"Key configured in registry: {GameUtility.FormatCodKey(codKey)}";
            MessageBox.Show(message, "Configured key", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}