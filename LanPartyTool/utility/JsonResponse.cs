using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    public class JsonResponse
    {
        public JsonResponse(bool success = true)
        {
            Success = success;
        }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; private set; }

        [JsonProperty(PropertyName = "data")] public object Data { get; private set; }

        [JsonProperty(PropertyName = "error")] public object Error { get; private set; }

        public static JsonResponse GetInstance(bool success = true)
        {
            return new JsonResponse(success);
        }

        public static JsonResponse GetSuccessInstance(object data = null)
        {
            return new JsonResponse {Data = data};
        }

        public static JsonResponse GetErrorInstance(string error = null)
        {
            return new JsonResponse(false) {Error = error};
        }
    }
}