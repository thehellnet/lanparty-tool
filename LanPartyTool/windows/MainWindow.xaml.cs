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
        private const int LogTextMaxLines = 100;

        private static readonly ILog Logger = LogManager.GetLogger(typeof(MainWindow));

        private readonly Config _config = Config.GetInstance();

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
            Dispatcher.Invoke((MethodInvoker) delegate
            {
                LogText.AppendText(message);

                while (LogText.LineCount > LogTextMaxLines)
                {
                    LogText.Text = LogText.Text.Remove(0, LogText.GetLineLength(0));
                }
            });
        }
    }
}