using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IChangeset
    {
        DateTime Timestamp { get; }
        List<FileChanges> FileChanges { get; }
        Dictionary<string, string> FileRenames { get;  }
    }
}