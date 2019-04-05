using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using System.Text.RegularExpressions;

namespace vcsparser.bugdatabase.azuredevops
{
    internal class AzureDevOps
    {
        private readonly ILogger logger;

        private readonly GetChangeset changeset;

        private readonly string organisation;
        private readonly string project;
        private readonly string team;
        private readonly string queryString;
        private readonly string personalAccessToken;
        private readonly RepoType repoType;

        private readonly IWebRequest webRequest;

        private Uri Uri {
            get {
                return new Uri($"https://dev.azure.com/{organisation}/{project}/{team}/_apis/wit/wiql?api-version=4.1");
            }
        }

        private AuthenticationHeaderValue Authorization {
            get {
                string pat = $":{personalAccessToken}";
                var bytes = Encoding.UTF8.GetBytes(pat);
                var base64 = Convert.ToBase64String(bytes);
                return new AuthenticationHeaderValue("Basic", $"{base64}");
            }
        }

        private Regex BugRegex {
            get {
                return new Regex(@"(?i)((?:^|\W)bug(?:$|\W)|(?:^|\W)crash(?:$|\W)|(?:^|\W)crashes(?:$|\W))|(?:^|\W)bugs(?:$|\W)|(?:^|\W)bugfix(?:$|\W)|(?:^|\W)bugfixes(?:$|\W)");
            }
        }

        internal AzureDevOps(ILogger logger, GetChangeset changeset, IWebRequest webRequest, DllArgs args)
        {
            this.logger = logger;
            this.changeset = changeset;
            this.webRequest = webRequest;
            this.organisation = args.Organisation;
            this.project = args.Project;
            this.team = args.Team;
            this.personalAccessToken = args.PersonalAccessToken;
            this.repoType = args.RepoType;
            this.queryString = string.Format(args.QueryString, args.From, args.To);
        }

        private async Task<JSONQuery> GetWorkItemList(Uri uri)
        {
            var json = JsonConvert.SerializeObject(new
            {
                query = this.queryString
            });
            var content = new StringContent(json, Encoding.UTF8, WebRequest.MEDIA_JSON);
            content.Headers.ContentType = new MediaTypeHeaderValue(WebRequest.MEDIA_JSON);

            HttpRequestMessage httpRequest = webRequest.NewHttpRequestMessage(uri, HttpMethod.Post);
            httpRequest.Content = content;
            return await SendRequest<JSONQuery>(httpRequest);
        }

        private async Task<dynamic> GetFullWorkItem(Uri uri)
        {
            HttpRequestMessage httpRequest = webRequest.NewHttpRequestMessage(uri, HttpMethod.Get);
            return await SendRequest<dynamic>(httpRequest);
        }

        private async Task<T> SendRequest<T>(HttpRequestMessage httpRequest)
        {
            httpRequest.Headers.Authorization = Authorization;
            var response = await webRequest.Send(httpRequest);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(responseString);
            }
            throw new Exception($"Response {response.StatusCode}. Content: {responseString}");
        }

        private WorkItem ProcessWorkItem(dynamic fullWorkItem)
        {
            var integrationBuild = (string)fullWorkItem.fields["Microsoft.VSTS.Build.IntegrationBuild"];
            var validChangeset = IsNumeric(integrationBuild);
            var message = "";
            var flaggedAsBug = false;
            DateTime? changesetDate = null;

            if (validChangeset)
            {
                if (repoType == RepoType.Perforce)
                {
                    var p4Changeset = changeset.ProcessPerforceRecord(Convert.ToInt32(integrationBuild));
                    if (p4Changeset != null)
                    {
                        if (p4Changeset.ChangesetNumber == 0)
                        {
                            validChangeset = false;
                        }
                        else
                        {
                            message = p4Changeset.ChangesetMessage;
                            flaggedAsBug = BugRegex.IsMatch(message);
                        }
                        changesetDate = p4Changeset.ChangesetTimestamp;
                    }
                }
                else if (repoType == RepoType.Git)
                {
                    // TODO Implement Git Process
                }
            }

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

        private bool IsNumeric(string number) => int.TryParse(number, out _);

        private IEnumerable<WorkItem> GetWorkItemList(JSONQueryItem[] items)
        {
            int count = 0;
            foreach (var item in items)
            {
                count++;
                logger.LogToConsole($"Processing Work Item {count}/{items.Length}");
                WorkItem workItem = null;
                try
                {
                    var fullWorkItem = GetFullWorkItem(item.url).Result;
                    workItem = ProcessWorkItem(fullWorkItem);
                }
                catch (Exception e)
                {
                    // TODO Decide if we want to log the errors or not?
                    if (e is AggregateException)
                        logger.LogToConsole($"Error: {e.InnerException.Message}");
                    else
                        logger.LogToConsole($"Error: {e.Message}");
                }
                if (workItem != null)
                    yield return workItem;
            }
        }

        public WorkItemList GetWorkItems()
        {
            logger.LogToConsole(this.Uri.ToString());
            var json = GetWorkItemList(this.Uri).Result;
            var itemList = GetWorkItemList(json.workItems).ToList();
            WorkItemList workItemList = new WorkItemList
            {
                TotalWorkItems = json.workItems.Length,
                WorkItems = itemList,
                ValidWorkItems = itemList.Where(item => item.ValidChangeset).Count(),
                ValidWorkItemsFlaggedAsBug = itemList.Where(item => item.FlaggedAsBug).Count()
            };
            return workItemList;
        }
    }
}
