using SelfHostingWebServer.Interfaces;

namespace SelfHostingWebServer.Implementations
{
    internal class DefaultSerializer : ISerializer
    {
        public string Serialize(object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
        }
    }
}
