using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using log4net.Config;

[assembly: AssemblyTitle("LanPartyTool")]
[assembly: AssemblyDescription("Custom helper tool for LAN parties")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("The HellNet.org")]
[assembly: AssemblyProduct("LanPartyTool")]
[assembly: AssemblyCopyright("Copyright ©  2018 - Luca Cireddu")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]

[assembly: AssemblyVersion("0.0.8.0")]
[assembly: AssemblyFileVersion("0.0.8.0")]

[assembly: XmlConfigurator(ConfigFile = "log4net.config")]