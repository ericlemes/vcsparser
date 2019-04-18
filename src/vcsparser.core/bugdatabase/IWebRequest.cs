using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IWebRequest
    {
        HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method);
        HttpRequestMessage NewHttpRequestMessage(Uri uri, HttpMethod method, string mediaType);
        Task<HttpResponseMessage> Send(HttpRequestMessage message);
    }
}
