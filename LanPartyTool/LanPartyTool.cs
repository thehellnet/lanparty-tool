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

            try
            {
                lanPartyTool.Run();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _agent.Start();
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mainWindow.Close();
            _agent.Stop();
            base.OnExit(e);
        }
    }
}