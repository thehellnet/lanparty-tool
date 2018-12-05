using System.Collections.Generic;

namespace LanPartyTool.agent
{
    public class Command
    {
        public string Action { get; set; }
        public Dictionary<string, string> Args { get; set; } = new Dictionary<string, string>();
    }
}