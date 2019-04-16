using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.git;
using vcsparser.core.p4;

namespace vcsparser.core.bugdatabase
{
    public interface IWorkItemConverter
    {
        IDictionary<DateTime, IDictionary<string, IChangeset>> Convert(IDictionary<DateTime, IDictionary<string, WorkItem>> items);
    }
}
