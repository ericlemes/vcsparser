using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.bugdatabase.azuredevops
{
    public interface IAzureDevOps
    {
        Dictionary<DateTime, Dictionary<string, WorkItem>> GetWorkItems();
    }
}
