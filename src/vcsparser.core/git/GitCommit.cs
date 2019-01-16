using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.git
{
    public class GitCommit : IChangeset
    {
        public List<FileChanges> ChangesetFileChanges { get; set; }

        public DateTime AuthorDate {
            get; set;
        }

        public DateTime ChangesetTimestamp
        {
            get { return this.CommiterDate; }
        }

        public string ChangesetAuthor
        {
            get { return this.Commiter; }
        }

        public string CommitHash { get; set; }

        public GitCommit()
        {
            this.ChangesetFileChanges = new List<FileChanges>();
            this.ChangesetFileRenames = new Dictionary<string, string>();
            this.ChangesetMessage = "";
        }

        public string Author { get; set; }
        public string Commiter { get; set; }
        public DateTime CommiterDate { get; set; }
        public Dictionary<string, string> ChangesetFileRenames { get; set; }

        public string ChangesetMessage { get; set; }

        public object ChangesetIdentifier { get { return CommitHash; } }

        public void AppendCommitMessage(string line)
        {
            ChangesetMessage += line + Environment.NewLine;            
        }
    }
}
