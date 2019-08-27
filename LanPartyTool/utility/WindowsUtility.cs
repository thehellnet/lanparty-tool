using System.Diagnostics;
using System.IO;
using WindowsInput;
using WindowsInput.Native;
using log4net;

namespace LanPartyTool.utility
{
    internal class WindowsUtility
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WindowsUtility));

        public static void EditFile(string path)

        {
            if (path == null) return;
            if (!File.Exists(path)) return;

            var editor = @"notepad.exe";

            const string notepadPlusPlus = @"C:\\Program Files\\Notepad++\\notepad++.exe";
            if (File.Exists(notepadPlusPlus))
            {
                editor = notepadPlusPlus;
            }

            var process = new Process {StartInfo = {FileName = editor, Arguments = path}};
            process.Start();
        }

        public static void OpenFileFolder(string path)

        {
            var directory = Path.GetDirectoryName(path);
            OpenFolder(directory);
        }

        public static void OpenFolder(string path)
        {
            if (path != null) Process.Start(path);
        }

        //public static void SendKeyDown(string processName = Constants.GameExeName)
        //{
        //    var processes = Process.GetProcessesByName(processName);
        //    foreach (var process in processes)
        //    {
        //        Logger.Debug($"PostMessage to process {process.Id} [{process.ProcessName}]");
        //        KeyboardUtility.Send(KeyboardUtility.ScanCodeShort.OEM_PERIOD);
        //    }
        //}

        public static void SendKeyDown()
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(VirtualKeyCode.OEM_PERIOD);
        }
    }
}