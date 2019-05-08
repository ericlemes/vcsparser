using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public class ApiConverter : IApiConverter
    {
        public WorkItem ConvertToWorkItem(dynamic fullWorkItem)
        {
            string integrationBuild = fullWorkItem.fields["Microsoft.VSTS.Build.IntegrationBuild"] ?? string.Empty;
            DateTime closedDate = DateTime.Parse((string)fullWorkItem.fields["Microsoft.VSTS.Common.ClosedDate"], CultureInfo.InvariantCulture);

            return new WorkItem
            {
                WorkItemId = fullWorkItem.id,
                ChangesetId = integrationBuild.Trim(),
                ClosedDate = closedDate
            };
        }
    }
}