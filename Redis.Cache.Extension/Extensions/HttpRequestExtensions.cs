using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Redis.Cache.Extension.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetUrlString(this HttpRequest request)
        {
            var uriBuilder = new UriBuilder
            {
                Scheme = request.Scheme,
                Host = request.Host.Host,
                Path = request.Path.ToString(),
                Query = request.QueryString.ToString()
            };

            return uriBuilder.Uri.ToString();
        }

        public static async Task<string> GetRequestBodyString(this HttpRequest request)
        {
            var body = string.Empty;
            request.Body.Position = 0;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, false, 1024, true))
            {
                body = await reader.ReadToEndAsync();
            }
            request.Body.Position = 0;

            // Body is empty here
            if (string.IsNullOrWhiteSpace(body))
            {
                return body;
            }

            // This will remove any formatting from body
            var requestBody = JsonSerializer.Deserialize<object>(body);

            return JsonSerializer.Serialize(requestBody);
        }
    }
}
