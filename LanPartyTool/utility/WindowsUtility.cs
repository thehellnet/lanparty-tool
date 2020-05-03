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

            var editor = Constants.NotepadExeFilename;
            if (File.Exists(Constants.NotepadPlusPlusExeFilepath)) editor = Constants.NotepadPlusPlusExeFilepath;

            var arguments = $"\"{path}\"";
            Process.Start(editor, arguments);
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

        public static void SendKeyDownExec()
        {
            SendKeyDown(VirtualKeyCode.OEM_PERIOD);
        }

        public static void SendKeyDownDump()
        {
            SendKeyDown(VirtualKeyCode.OEM_COMMA);
        }

        private static void SendKeyDown(VirtualKeyCode virtualKeyCode)
        {
            var inputSimulator = new InputSimulator();
            inputSimulator.Keyboard.KeyPress(virtualKeyCode);
        }
    }
}