using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace vcsparser.core.bugdatabase
{
    public interface IWebRequest
    {
        HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method);
        HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method, string mediaType);
        Task<HttpResponseMessage> Send(HttpRequestMessage message);
    }

    public class WebRequest : IWebRequest
    {
        private static readonly Lazy<HttpClient> httpClient = new Lazy<HttpClient>(() => new HttpClient());

        public static readonly string MEDIA_JSON = @"application/json";

        public HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method) => NewHttpRequestMessage(uri, method, MEDIA_JSON);
        public HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method, string mediaType)
        {
            HttpRequestMessage httpRequestMessage = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = method
            };
            httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mediaType));
            return httpRequestMessage;
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessage message)
        {
            return await httpClient.Value.SendAsync(message);
        }
    }
}
