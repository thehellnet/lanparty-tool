using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using LanPartyTool.log;

namespace LanPartyTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Logger logger = Logger.GetLogger(typeof(App));

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            logger.info(" ---------- ############ START ############ ---------- ");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            logger.info(" ---------- ############  END  ############ ---------- ");
            base.OnExit(e);
        }
    }
}