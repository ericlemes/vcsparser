using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase.azuredevops
{
    internal class JSONQueryColumn
    {
        public string referenceName { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    internal class JSONQueryItem
    {
        public string id { get; set; }
        public Uri url { get; set; }
    }

    internal class JSONQuery
    {
        public string queryType { get; set; }
        public string queryResultType { get; set; }
        public string asOf { get; set; }
        public JSONQueryColumn[] columns { get; set; }
        public JSONQueryItem[] workItems { get; set; }
    }
}
