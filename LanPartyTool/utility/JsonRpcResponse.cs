using LanPartyTool.agent;
using Newtonsoft.Json;

namespace LanPartyTool.utility
{
    internal class JsonRpcResponse
    {
        [JsonProperty(PropertyName = "jsonrpc")]
        public string JsonRpc { get; set; }

        [JsonProperty(PropertyName = "result")]
        public JsonResponse Result { get; set; }

        [JsonProperty(PropertyName = "error")] public object Error { get; set; }

        [JsonProperty(PropertyName = "id")] public object Id { get; set; }
    }
}