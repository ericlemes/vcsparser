using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace vcsparser.bugdatabase.azuredevops
{
    internal class AzureDevOps
    {
        private readonly string Organization;
        private readonly string Project;
        private readonly string Team;
        private readonly string QueryString;
        private readonly string PersonalAccessToken;

        private readonly WebRequest request;

        private Uri Uri {
            get {
                return new Uri($"https://dev.azure.com/{Organization}/{Project}/{Team}/_apis/wit/wiql?api-version=4.1");
            }
        }

        private const string application = "application/json";

        private AuthenticationHeaderValue Authorization {
            get {
                string pat = $":{PersonalAccessToken}";
                var bytes = Encoding.UTF8.GetBytes(pat);
                var base64 = Convert.ToBase64String(bytes);
                return new AuthenticationHeaderValue("Basic", $"{base64}");
            }
        }

        private HttpContent HttpContent {
            get {
                var json = JsonConvert.SerializeObject(new
                {
                    query = this.QueryString
                });
                var content = new StringContent(json, Encoding.UTF8, application);
                content.Headers.ContentType = new MediaTypeHeaderValue(application);
                return content;
            }
        }

        internal AzureDevOps(string organization, string project, string team, string personalAccessToken, string query, string from, string to)
        {
            this.Organization = organization;
            this.Project = project;
            this.Team = team;
            this.PersonalAccessToken = personalAccessToken;
            this.QueryString = string.Format(query, from, to);
            this.request = new WebRequest();
        }

        private JSONQuery PostQuery(Uri uri)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Post,
                Content = HttpContent
            };
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(application));
            return SendRequest<JSONQuery>(httpRequest);
        }

        private dynamic GetWorkItem(Uri uri)
        {
            HttpRequestMessage httpRequest = new HttpRequestMessage
            {
                RequestUri = uri,
                Method = HttpMethod.Get
            };
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(application));
            return SendRequest(httpRequest);
        }

        private WorkItem ProcessWorkItem(dynamic fullWorkItem)
        {
            var integrationBuild = (string)fullWorkItem.fields["Microsoft.VSTS.Build.IntegrationBuild"];
            var validChangeset = IsNumeric(integrationBuild);
            var message = "";
            var flaggedAsBug = false;
            DateTime? changesetDate = null;

            // TODO Work out if p4 or Git
            // TODO Get Changeset

            return new WorkItem
            {
                WorkItemId = fullWorkItem.id,
                IntegrationBuild = integrationBuild,
                ClosedDate = fullWorkItem.fields["Microsoft.VSTS.Common.ClosedDate"],
                ValidChangeset = validChangeset,
                Message = message,
                FlaggedAsBug = flaggedAsBug,
                ChangesetDate = changesetDate
            };
        }

        private bool IsNumeric(string number)
        {
            if (int.TryParse(number, out int i))
                return true;
            return false;
        }

        private dynamic SendRequest(HttpRequestMessage httpRequest) => SendRequest<dynamic>(httpRequest);
        private T SendRequest<T>(HttpRequestMessage httpRequest)
        {
            httpRequest.Headers.Authorization = Authorization;
            var response = request.Send(httpRequest).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            throw new Exception($"Response {response.StatusCode}. Content: {responseString}");
        }

        public WorkItemList Query()
        {
            Console.WriteLine(this.Uri);
            var json = PostQuery(this.Uri);
            var workItemList = new WorkItemList
            {
                TotalWorkItems = json.workItems.Length,
                WorkItems = new List<WorkItem>()
            };
            for (int i = 0; i < json.workItems.Length; i++)
            {
                try
                {
                    Console.Write($"Work Item {i + 1}/{json.workItems.Length}");
                    var listItem = json.workItems[i];
                    var workItemFull = GetWorkItem(listItem.url);
                    WorkItem workItem = ProcessWorkItem(workItemFull);

                    if (workItem.ValidChangeset)
                        workItemList.ValidWorkItems++;
                    
                    if (workItem.FlaggedAsBug)
                        workItemList.ValidWorkItemsFlaggedAsBug++;

                    (workItemList.WorkItems as List<WorkItem>).Add(workItem);
                }
                catch (Exception e)
                {
                    Console.Write($" Error: {e.Message}");
                }
                finally
                {
                    Console.WriteLine();
                }
            }
            return workItemList;
        }
    }
}
