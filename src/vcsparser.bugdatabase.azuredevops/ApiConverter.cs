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
        private string changesetIdFieldName;

        private string closedDateFieldName;

        public ApiConverter(string changesetIdFieldName, string closedDateFieldName)
        {
            this.changesetIdFieldName = changesetIdFieldName;
            this.closedDateFieldName = closedDateFieldName;
        }

        public WorkItem ConvertToWorkItem(dynamic fullWorkItem)
        {
            string integrationBuild = fullWorkItem.fields[changesetIdFieldName] ?? string.Empty;
            DateTime closedDate = DateTime.Parse((string)fullWorkItem.fields[closedDateFieldName], CultureInfo.InvariantCulture);

            return new WorkItem
            {
                WorkItemId = fullWorkItem.id,
                ChangesetId = integrationBuild.Trim(),
                ClosedDate = closedDate
            };
        }
    }
}