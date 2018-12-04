using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using log4net;
using log4net.Repository.Hierarchy;
using LanPartyTool.agent;

namespace LanPartyTool
{
    public class LanPartyTool : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LanPartyTool));

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
            Logger.Info("--------########  START  ########--------");

            _agent.Start();

            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mainWindow.Close();

            _agent.Stop();

            Logger.Info("--------########   END   ########--------");
            base.OnExit(e);
        }
    }
}