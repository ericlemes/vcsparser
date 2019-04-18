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
        IEnumerable<IChangeset> Convert(IEnumerable<WorkItem> items);
    }
}
