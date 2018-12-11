namespace LanPartyTool.agent
{
    public static class Protocol
    {
        public static class Command
        {
            public const string Ping = "ping";
            public const string WriteCfg = "writeCfg";
        }

        public static class Args
        {
            public const string CfgLines = "cfgLines";
        }
    }
}