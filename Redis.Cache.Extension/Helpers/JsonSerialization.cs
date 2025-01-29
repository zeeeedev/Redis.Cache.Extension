using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Redis.Cache.Extension.Helpers
{
    public static class JsonSerialization
    {
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };
    }
}
