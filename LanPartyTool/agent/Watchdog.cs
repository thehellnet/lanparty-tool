using System;
using System.Threading;
using log4net;

namespace LanPartyTool.agent
{
    internal class Watchdog
    {
        public delegate void WatchdogTimeoutHandler();

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Watchdog));

        private readonly int interval;

        private readonly object sync = new object();
        private readonly int timeout;
        private bool keepRunning;
        private DateTime lastPingDateTime = DateTime.Now;

        private Thread thread;

        public Watchdog(int interval = 250, int timeout = 1000)
        {
            this.interval = interval;
            this.timeout = timeout;
        }

        public event WatchdogTimeoutHandler OnWatchdogTimeout;

        public void Start()
        {
            lock (sync)
            {
                if (thread != null) return;

                thread = new Thread(Loop);

                keepRunning = true;
                lastPingDateTime = DateTime.Now;

                thread.Start();

                Logger.Info("Watchdog started");
            }
        }

        public void Stop()
        {
            lock (sync)
            {
                if (thread == null) return;

                keepRunning = false;

                if (thread.IsAlive) thread.Interrupt();

                thread.Join();
                thread = null;

                Logger.Info("Watchdog stopped");
            }
        }

        public void Ping()
        {
            lock (sync)
            {
                Logger.Debug("Watchdog ping");
                lastPingDateTime = DateTime.Now;
            }
        }

        private void Loop()
        {
            Logger.Info($"Watchdog started with interval of {interval} ms and timeout of {timeout} ms");

            while (keepRunning)
            {
                Thread.Sleep(interval);
                if (lastPingDateTime.AddMilliseconds(timeout) >= DateTime.Now) continue;

                Logger.Info("Watchdog timeout");

                new Thread(() => { OnWatchdogTimeout?.Invoke(); }).Start();
                keepRunning = false;
                break;
            }
        }
    }
}