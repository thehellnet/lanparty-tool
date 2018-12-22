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
        private bool Success { get; set; }

        [JsonProperty(PropertyName = "data")] private object Data { get; set; }

        [JsonProperty(PropertyName = "error")] private object Error { get; set; }

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