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

        public DateTime Timestamp { get; set; }

        public string ChangeDescrition { get; set; }

        public List<FileChanges> FileChanges { get; set; }

        public Dictionary<string, string> FileRenames { get; set; }


        public PerforceChangeset()
        {
            this.FileChanges = new List<FileChanges>();
            this.FileRenames = new Dictionary<string, string>();
        }
    }
}
