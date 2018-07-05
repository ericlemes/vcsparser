using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class Changeset
    {
        public int ChangesetNumber { get; set; }

        public string Author { get; set; }

        public string AuthorName { get; set; }

        public string AuthorWorkspace { get; set; }

        public DateTime Timestamp { get; set; }

        public string ChangeDescrition { get; set; }

        public List<FileChanges> FileChanges { get; set; }


        public Changeset()
        {
            this.FileChanges = new List<FileChanges>();
        }
    }
}
