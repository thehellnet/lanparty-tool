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

        protected bool Equals(JsonResponse other)
        {
            return Success == other.Success && Equals(Data, other.Data) && Equals(Error, other.Error);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((JsonResponse) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Success.GetHashCode();
                hashCode = (hashCode * 397) ^ (Data != null ? Data.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}