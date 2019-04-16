using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public interface IApiConverter
    {
        WorkItem ConvertToWorkItem(dynamic fullWorkItem);
    }

    public class ApiConverter : IApiConverter
    {
        public WorkItem ConvertToWorkItem(dynamic fullWorkItem)
        {
            string integrationBuild = fullWorkItem.fields["Microsoft.VSTS.Build.IntegrationBuild"];
            DateTime closedDate = DateTime.Parse((string)fullWorkItem.fields["Microsoft.VSTS.Common.ClosedDate"], CultureInfo.InvariantCulture);

            var workItem =  new WorkItem
            {
                WorkItemId = fullWorkItem.id,
                ChangesetId = integrationBuild
            };
            workItem.SetClosedDateFromDateTime(closedDate);
            return workItem;
        }
    }
}
