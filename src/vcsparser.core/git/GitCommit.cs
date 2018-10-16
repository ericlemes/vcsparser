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
        public List<FileChanges> FileChanges { get; set; }

        public DateTime AuthorDate {
            get; set;
        }

        public DateTime Timestamp
        {
            get { return this.CommiterDate; }
        }

        public string CommitHash { get; set; }

        public GitCommit()
        {
            this.FileChanges = new List<FileChanges>();
            this.FileRenames = new Dictionary<string, string>();
            this.Message = "";
        }

        public string Author { get; set; }
        public string Commiter { get; set; }
        public DateTime CommiterDate { get; set; }
        public Dictionary<string, string> FileRenames { get; set; }

        public string Message { get; set; }

        public object CommitIdentifier { get { return CommitHash; } }

        public void AppendCommitMessage(string line)
        {
            Message += line + Environment.NewLine;            
        }
    }
}
