using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

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