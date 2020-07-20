using System.Net;
using System.Reflection;

namespace LoPatcher.LanguageUpdate
{
    public class HttpClient : WebClient
    {
        public HttpClient()
        {
            Headers.Add("User-Agent", BuildUserAgent());
        }

        private static string BuildUserAgent()
        {
            var assemblyName = Assembly.GetExecutingAssembly().GetName();

            return $"{assemblyName.Name}-{assemblyName.Version}";
        }
    }
}