using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
}
