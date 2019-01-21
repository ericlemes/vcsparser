using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IChangeset
    {
        object ChangesetIdentifier { get; }
        DateTime ChangesetTimestamp { get; }
        List<FileChanges> ChangesetFileChanges { get; }
        Dictionary<string, string> ChangesetFileRenames { get;  }
        string ChangesetMessage { get; }        
        string ChangesetAuthor { get; }
    }
}