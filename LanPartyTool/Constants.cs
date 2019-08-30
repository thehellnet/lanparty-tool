namespace LanPartyTool
{
    internal static class Constants
    {
        public const string ToolCfgExecName = "lanpartytool";
        public const string ToolCfgDumpName = "lanpartydump";

        public const string CommandExecKey = ".";
        public const string CommandDumpKey = ",";

        public const string GameExeName = "iw3mp";

        public const string ToolCfgExecFilename = ToolCfgExecName + ".cfg";
        public const string ToolCfgDumpFilename = ToolCfgDumpName + ".cfg";

        public const string GameExeFilename = GameExeName + ".exe";

        public const string NotepadExeFilename = @"notepad.exe";
        public const string NotepadPlusPlusExeFilepath = @"C:\\Program Files\\Notepad++\\notepad++.exe";

        public static readonly string[] Cod4RegistryPaths =
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Activision\Call of Duty 4",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Activision\Call of Duty 4"
        };

        public const int SerialBarcodeDebounceTimeout = 2;

        public const int barcodeCountsExec = 1;
        public const int barcodeCountsDump = 2;
    }
}