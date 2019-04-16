using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public interface IAzureDevOpsRequest
    {
        AuthenticationHeaderValue Authorization { get; }
        Uri WorkItemListUri { get; }
        string JsonQuery { get; }

        Task<JSONQuery> GetWorkItemList();
        Task<dynamic> GetFullWorkItem(Uri uri);
        Task<T> SendRequest<T>(HttpRequestMessage httpRequest);
    }

    public class AzureDevOpsRequest : IAzureDevOpsRequest
    {
        private readonly DllArgs args;

        private readonly IWebRequest webRequest;

        public AzureDevOpsRequest(IWebRequest webRequest, DllArgs args)
        {
            this.webRequest = webRequest;
            this.args = args;
        }

        public AuthenticationHeaderValue Authorization {
            get {
                string pat = $":{args.PersonalAccessToken}";
                var bytes = Encoding.UTF8.GetBytes(pat);
                var base64 = Convert.ToBase64String(bytes);
                return new AuthenticationHeaderValue("Basic", base64);
            }
        }

        public Uri WorkItemListUri {
            get {
                return new Uri($"https://dev.azure.com/{args.Organisation}/{args.Project}/{args.Team}/_apis/wit/wiql?api-version=4.1");
            }
        }

        public string JsonQuery {
            get {
                return JsonConvert.SerializeObject(new
                {
                    query = string.Format(args.QueryString, args.From, args.To)
                });
            }
        }

        public async Task<JSONQuery> GetWorkItemList()
        {
            var content = new StringContent(JsonQuery, Encoding.UTF8, WebRequest.MEDIA_JSON);
            content.Headers.ContentType = new MediaTypeHeaderValue(WebRequest.MEDIA_JSON);

            HttpRequestMessage httpRequest = webRequest.NewHttpRequestMessage(WorkItemListUri, HttpMethod.Post);
            httpRequest.Content = content;
            return await SendRequest<JSONQuery>(httpRequest);
        }

        public async Task<dynamic> GetFullWorkItem(Uri uri)
        {
            HttpRequestMessage httpRequest = webRequest.NewHttpRequestMessage(uri, HttpMethod.Get);
            return await SendRequest<dynamic>(httpRequest);
        }

        public async Task<T> SendRequest<T>(HttpRequestMessage httpRequest)
        {
            httpRequest.Headers.Authorization = Authorization;
            var response = await webRequest.Send(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<T>(responseString);

            throw new Exception($"Response {response.StatusCode}. Content: {responseString}");
        }
    }
}
