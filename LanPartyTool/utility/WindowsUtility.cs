using System.Diagnostics;
using System.IO;
using log4net;

namespace LanPartyTool.utility
{
    internal class WindowsUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WindowsUtility));

        public static void OpenFileFolder(string path)

        {
            var directory = Path.GetDirectoryName(path);
            OpenFolder(directory);
        }

        public static void OpenFolder(string path)
        {
            if (path != null) Process.Start(path);
        }

        public static void SendKeyDown(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            foreach (var process in processes)
            {
                Logger.Debug($"PostMessage to process {process.Id} [{process.ProcessName}]");
                KeyboardUtility.Send(KeyboardUtility.ScanCodeShort.OEM_PERIOD);
            }
        }
    }
}