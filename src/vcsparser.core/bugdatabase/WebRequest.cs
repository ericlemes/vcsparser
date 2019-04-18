using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace vcsparser.core.bugdatabase
{
    public class WebRequest : IWebRequest
    {
        IHttpClientWrapperFactory httpClientWrapperFactory;

        public static readonly string MEDIA_JSON = @"application/json";

        public WebRequest(IHttpClientWrapperFactory httpClientWrapperFactory)
        {
            this.httpClientWrapperFactory = httpClientWrapperFactory;
        }

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
            return await this.httpClientWrapperFactory.GetSingletonHttpClientWrapper().SendAsync(message);
        }
    }
}
