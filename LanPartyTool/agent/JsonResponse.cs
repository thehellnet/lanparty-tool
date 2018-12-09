namespace LanPartyTool.agent
{
    class JsonResponse
    {
        public bool success { get; set; } = true;
        public object data { get; set; } = null;
        public object error { get; set; } = null;

        private JsonResponse(bool success = true)
        {
            this.success = success;
        }

        public static JsonResponse GetInstance(bool success = true)
        {
            return new JsonResponse(success);
        }

        public static JsonResponse GetSuccessInstance(object data)
        {
            return new JsonResponse(true) {data = data};
        }

        public static JsonResponse GetErrorInstance(string error)
        {
            return new JsonResponse(false) {error = error};
        }
    }
}