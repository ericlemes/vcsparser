using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public abstract class BugDatabaseProvider
    {
        protected readonly ILogger logger;

        public BugDatabaseProvider(ILogger logger) {
            this.logger = logger;
        }

        public abstract int ProcessArgs(string[] args);
        public abstract WorkItemList Run();
    }
}
