using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace vcsparser.bugdatabase
{
    public class WebRequest
    {
        private static readonly Lazy<HttpClient> _httpClient = new Lazy<HttpClient>(() => new HttpClient());

        private HttpClient httpClient {
            get {
                return _httpClient.Value;
            }
        }

        public async Task<HttpResponseMessage> Send(HttpRequestMessage message)
        {
            return await httpClient.SendAsync(message);
        }
    }
}
