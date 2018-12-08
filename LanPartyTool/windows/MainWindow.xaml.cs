using System.Windows;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Threading;
using log4net;
using LanPartyTool.config;
using Binding = System.Windows.Data.Binding;
using TextBox = System.Windows.Controls.TextBox;

namespace LanPartyTool.windows
{
    public partial class MainWindow : Window
    {
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

            CfgFileText.SetBinding(TextBox.TextProperty, new Binding()
            {
                Path = new PropertyPath("CfgFile"),
                Source = _config,
                Mode = BindingMode.OneWay
            });

            LogEvent.OnLogEvent += NewLogEvent;
        }

        private void NewLogEvent(string message)
        {
            Dispatcher.Invoke((MethodInvoker) delegate
            {
                LogText.AppendText(message);
                while (LogText.LineCount > 100)
                {
                    LogText.Text = LogText.Text.Remove(0, LogText.GetLineLength(0));
                }
            });
        }
    }
}