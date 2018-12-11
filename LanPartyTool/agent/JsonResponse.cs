namespace LanPartyTool.agent
{
    internal class JsonResponse
    {
        private bool Success { get; set; }
        private object Data { get; set; }
        private object Error { get; set; }

        private JsonResponse(bool success = true)
        {
            Success = success;
        }

        public static JsonResponse GetInstance(bool success = true)
        {
            return new JsonResponse(success);
        }

        public static JsonResponse GetSuccessInstance(object data)
        {
            return new JsonResponse {Data = data};
        }

        public static JsonResponse GetErrorInstance(string error)
        {
            return new JsonResponse(false) {Error = error};
        }
    }
}