using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using log4net;
using LanPartyTool.agent;
using LanPartyTool.config;
using LanPartyTool.windows;

namespace LanPartyTool
{
    public class LanPartyTool : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LanPartyTool));

        private readonly Config _config = Config.GetInstance();

        private readonly Agent _agent = new Agent();
        private readonly MainWindow _mainWindow = new MainWindow();

        [STAThread]
        public static void Main()
        {
            var lanPartyTool = new LanPartyTool();
            lanPartyTool.Run();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _mainWindow.Show();
            _agent.Start();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _agent.Stop();
            _mainWindow.Close();
            base.OnExit(e);
        }
    }
}