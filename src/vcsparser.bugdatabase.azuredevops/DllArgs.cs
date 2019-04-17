using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase.azuredevops
{
    public class DllArgs
    {
        [Option("organisation", HelpText = "Organisation of where project exists", Required = true)]
        public string Organisation { get; set; }

        [Option("project", HelpText = "Project to query", Required = true)]
        public string Project { get; set; }

        [Option("team", HelpText = "Team Project", Required = true)]
        public string Team { get; set; }

        [Option("query", HelpText = "Query to select work Items", Required = false, Default = @"Select [System.Id] From WorkItems Where [System.WorkItemType] = 'Bug' AND [System.State] = 'Closed' AND [Microsoft.VSTS.Common.ResolvedReason] = 'Fixed' and [Microsoft.VSTS.Common.ClosedDate] >= '{0}' and [Microsoft.VSTS.Common.ClosedDate] <= '{1}'")]
        public string QueryString { get; set; }

        [Option("from", HelpText = "Start Date in format 'yyyy-mm-dd'", Required = true)]
        public string From { get; set; }

        [Option("to", HelpText = "End Date in format 'yyyy-mm-dd'", Required = true)]
        public string To { get; set; }

        [Option("token", HelpText = "Azure DevOps Personal Access Token", Required = true)]
        public string PersonalAccessToken { get; set; }
    }
}
