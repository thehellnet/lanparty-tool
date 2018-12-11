using Newtonsoft.Json;

namespace LanPartyTool.agent
{
    public class JsonResponse
    {
        [JsonProperty]
        private bool Success { get; set; }

        [JsonProperty]
        private object Data { get; set; }

        [JsonProperty]
        private object Error { get; set; }

        private JsonResponse(bool success = true)
        {
            Success = success;
        }

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