using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IBugDatabaseCommandLineArgs
    {
        string BugRegexes { get; set; }

        string BugDatabaseDLL { get; set; }

        IEnumerable<string> BugDatabaseDllArgs { get; set; }
    }
}
