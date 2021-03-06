﻿using System;
using System.Windows;
using LanPartyTool.agent;
using LanPartyTool.config;
using LanPartyTool.windows;
using log4net;

namespace LanPartyTool
{
    public class LanPartyTool : Application
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(LanPartyTool));

        private readonly Agent _agent = new Agent();

        private readonly Config _config = Config.GetInstance();
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
                Logger.Error(e.Message);
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _mainWindow.OnAgentRestart += AgentRestartHandler;
            _mainWindow.OnManualBarcode += ManualBarcodeHandler;
            _mainWindow.OnDumpConfig += DumpConfigHandler;

            _agent.Start();
            _mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mainWindow.Close();
            _agent.Stop();

            _mainWindow.OnAgentRestart -= AgentRestartHandler;

            base.OnExit(e);
        }

        private void AgentRestartHandler()
        {
            _agent.Restart();
        }

        private void ManualBarcodeHandler(string barcode)
        {
            _agent.NewBarcodeHandler(barcode, Constants.barcodeCountsExec);
        }

        private void DumpConfigHandler()
        {
            _agent.DumpConfigHandler();
        }
    }
}