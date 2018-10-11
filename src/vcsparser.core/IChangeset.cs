using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IChangeset
    {
        object CommitIdentifier { get; }
        DateTime Timestamp { get; }
        List<FileChanges> FileChanges { get; }
        Dictionary<string, string> FileRenames { get;  }
        string Message { get; }
    }
}