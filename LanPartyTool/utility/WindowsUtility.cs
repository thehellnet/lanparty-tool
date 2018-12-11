using System.Diagnostics;
using System.IO;

namespace LanPartyTool.utility
{
    internal class WindowsUtility
    {
        public static void OpenFileFolder(string path)
        {
            var directory = Path.GetDirectoryName(path);
            OpenFolder(directory);
        }

        public static void OpenFolder(string path)
        {
            if (path != null)
            {
                Process.Start(path);
            }
        }
    }
}