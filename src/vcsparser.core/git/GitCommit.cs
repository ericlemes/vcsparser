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
            get { return this.AuthorDate; }
        }

        public string CommitHash { get; set; }

        public GitCommit()
        {
            this.FileChanges = new List<FileChanges>();
            this.FileRenames = new Dictionary<string, string>();
        }

        public string Author { get; set; }
        public string Commiter { get; set; }
        public DateTime CommiterDate { get; set; }
        public Dictionary<string, string> FileRenames { get; set; }

        public string CommitMessage = "";

        public void AppendCommitMessage(string line)
        {
            CommitMessage += line + Environment.NewLine;            
        }
    }
}
