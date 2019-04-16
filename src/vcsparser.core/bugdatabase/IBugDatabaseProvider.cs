using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IBugDatabaseProvider
    {
        void SetLogger(ILogger logger);
        void SetWebRequest(IWebRequest webRequest);

        int ProcessArgs(IEnumerable<string> args);
        WorkItemList Process();
    }
}
