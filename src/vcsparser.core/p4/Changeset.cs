using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.p4
{
    public class PerforceChangeset : IChangeset
    {
        public int ChangesetNumber { get; set; }

        public string Author { get; set; }

        public string AuthorName { get; set; }

        public string AuthorWorkspace { get; set; }

        public DateTime ChangesetTimestamp { get; set; }

        public string ChangesetMessage { get; set; }

        public List<FileChanges> ChangesetFileChanges { get; set; }

        public Dictionary<string, string> ChangesetFileRenames { get; set; }

        public object ChangesetIdentifier
        {
            get { return ChangesetNumber; }
        }

        public string ChangesetAuthor
        {
            get { return this.AuthorName; }
        }

        public PerforceChangeset()
        {
            this.ChangesetFileChanges = new List<FileChanges>();
            this.ChangesetFileRenames = new Dictionary<string, string>();
        }

        internal void AppendMessage(string msg)
        {
            ChangesetMessage += msg + Environment.NewLine;
        }
    }
}
