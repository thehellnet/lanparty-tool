using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace LanPartyTool.utility
{
    class GameUtility
    {
        private static readonly string[] Cod4RegistryPaths =
        {
            @"HKEY_LOCAL_MACHINE\SOFTWARE\Activision\Call of Duty 4",
            @"HKEY_LOCAL_MACHINE\SOFTWARE\WOW6432Node\Activision\Call of Duty 4"
        };

        public static string DefaultGameExe()
        {
            var installPath = DefaultInstallationDir();
            if (installPath == "")
            {
                return "";
            }

            var gameExePath = Path.Combine(installPath, "iw3mp.exe");
            return File.Exists(gameExePath) ? gameExePath : "";
        }

        public static string DefaultToolCfg()
        {
            var gameExePath = DefaultGameExe();
            if (gameExePath == "")
            {
                return "";
            }

            var installFolderPath = Path.GetDirectoryName(gameExePath);
            if (installFolderPath == null)
            {
                return "";
            }

            var installFolderRealPath = installFolderPath.Substring(3);
            var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var virtualStoreInstallFolder = Path.Combine(localPath, "VirtualStore", installFolderRealPath);
            if (!Directory.Exists(virtualStoreInstallFolder))
            {
                return "";
            }

            var cfgFileFolder = Path.Combine(virtualStoreInstallFolder, "main");
            var toolCfg = Path.Combine(cfgFileFolder, Constants.ToolCfgFilename);
            return toolCfg;
        }

        public static string DefaultProfileCfg()
        {
            var gameExePath = DefaultGameExe();
            if (gameExePath == "")
            {
                return "";
            }

            var installFolderPath = Path.GetDirectoryName(gameExePath);
            if (installFolderPath == null)
            {
                return "";
            }

            var installFolderRealPath = installFolderPath.Substring(3);
            var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var virtualStoreInstallFolder = Path.Combine(localPath, "VirtualStore", installFolderRealPath);
            var profilesFolder = Path.Combine(virtualStoreInstallFolder, "players", "profiles");
            if (!Directory.Exists(profilesFolder))
            {
                return "";
            }

            var activeProfileFile = Path.Combine(profilesFolder, "active.txt");
            if (!File.Exists(activeProfileFile))
            {
                return "";
            }

            var activeProfileName = File.ReadAllText(activeProfileFile);
            var profileCfg = Path.Combine(profilesFolder, activeProfileName, "config_mp.cfg");
            return File.Exists(profileCfg) ? profileCfg : "";
        }

        public static string DefaultInstallationDir()
        {
            foreach (var registryPath in Cod4RegistryPaths)
            {
                var installPath = (string) Registry.GetValue(registryPath, "InstallPath", null);
                if (installPath != null && Directory.Exists(installPath))
                {
                    return installPath;
                }
            }

            return "";
        }

        public static string ReadCodKey()
        {
            foreach (var registryPath in Cod4RegistryPaths)
            {
                var codkey = (string) Registry.GetValue(registryPath, "codkey", null);
                if (codkey != null && codkey.Length == 20)
                {
                    return codkey.ToUpper();
                }
            }

            return "";
        }

        public static string FormatCodKey(string codKey)
        {
            var items = new List<string>();
            for (var i = 0; i < codKey.Length; i += 4)
            {
                items.Add(codKey.Substring(i, 4));
            }

            return string.Join("-", items);
        }

        public static List<string> ReadCfg(string cfgPath)
        {
            var rows = new List<string>();
            if (!File.Exists(cfgPath))
            {
                return rows;
            }

            var rawConent = File.ReadAllText(cfgPath);
            var lines = rawConent.Split(
                new[] {"\r\n", "\r", "\n"},
                StringSplitOptions.None
            );

            rows.AddRange(lines.Select(line => line.Trim()));

            return rows;
        }

        public static void WriteCfg(string cfgPath, List<string> rows)
        {
            var rawContent = string.Join("\n", rows);
            File.WriteAllText(cfgPath, rawContent);
        }

        public static string[] SplitCfgLine(string line)
        {
            return Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                .Cast<Match>()
                .Select(m => m.Value)
                .ToArray();
        }
    }
}