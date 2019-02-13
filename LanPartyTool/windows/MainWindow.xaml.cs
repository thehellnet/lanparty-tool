using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.IO.Ports;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public delegate void AgentRestartHandler();

        private const int MaxLogLines = 100;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));
        private readonly Config _config = Config.GetInstance();

        private readonly List<dynamic> _logList = new List<dynamic>();

        private readonly Status _status = Status.GetInstance();

        private readonly DispatcherTimer clockTimer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();

            Title = $"{Application.ResourceAssembly.GetName().Name}";

            VersionText.Text = $"ver. {Application.ResourceAssembly.GetName().Version}";

            clockTimer.Tick += (sender, args) =>
                ClockText.Text = $"{DateTime.Now}";
            clockTimer.Interval = TimeSpan.FromSeconds(1);
            clockTimer.Start();

            ClockText.Text = $"{DateTime.Now}";
            SerialPortComboBox.ItemsSource = SerialPort.GetPortNames();

            SocketStatusChangedHandler();
            SerialPortStatusChangedHandler();

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

            ServerUrlText.SetBinding(TextBox.TextProperty, new Binding
            {
                Path = new PropertyPath("ServerUrl"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            SerialPortComboBox.SetBinding(Selector.SelectedItemProperty, new Binding
            {
                Path = new PropertyPath("SerialPort"),
                Source = _config,
                Mode = BindingMode.TwoWay
            });

            LogEvent.OnLogEvent += NewLogEvent;
            _status.OnSocketStatusChanged += SocketStatusChangedHandler;
            _status.OnSerialPortStatusChanged += SerialPortStatusChangedHandler;
        }

        public event AgentRestartHandler OnAgentRestart;

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
            while (_logList.Count > MaxLogLines) _logList.RemoveAt(0);

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

        private void SerialPortStatusChangedHandler()
        {
            SerialPortStatusText.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(() =>
                {
                    switch (_status.SerialPortStatus)
                    {
                        case SerialPortReader.PortStatus.Closed:
                            SerialPortStatusText.Text = "Closed";
                            break;
                        case SerialPortReader.PortStatus.Preparing:
                            SerialPortStatusText.Text = "Preparing";
                            break;
                        case SerialPortReader.PortStatus.Open:
                            SerialPortStatusText.Text = "Open";
                            break;
                        case SerialPortReader.PortStatus.Parsing:
                            SerialPortStatusText.Text = "Parsing";
                            break;
                        case SerialPortReader.PortStatus.Closing:
                            SerialPortStatusText.Text = "Closing";
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

        private void RunGameButton_Click(object sender, RoutedEventArgs e)
        {
            LaunchServer();
        }

        private void LaunchServer()
        {
            var addresses = Dns.GetHostAddresses(_config.ServerUrl);
            var serverAddress = addresses[0].ToString();

            var arguments = " +connect " + serverAddress;

            var directoryInfo = new FileInfo(_config.GameExe).Directory;
            if (directoryInfo == null) return;

            var workingDirectory = directoryInfo.FullName;

            var process = new ProcessStartInfo(_config.GameExe)
            {
                WorkingDirectory = workingDirectory,
                Arguments = arguments,
                UseShellExecute = true
            };

            try
            {
                Process.Start(process);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RestartAgentButton_Click(object sender, RoutedEventArgs e)
        {
            new Thread(() => { OnAgentRestart?.Invoke(); }).Start();
        }
    }
}