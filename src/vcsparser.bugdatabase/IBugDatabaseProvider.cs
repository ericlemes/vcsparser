using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase
{
    public interface IBugDatabaseProvider
    {
        bool ProcessArgs(string[] args);
        WorkItemList Run();
    }
}
