using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase.azuredevops
{
    //public class JSONQueryColumn
    //{
    //    public string ReferenceName { get; set; }
    //    public string Name { get; set; }
    //    public string Url { get; set; }
    //}

    public class JSONQueryItem
    {
        public string Id { get; set; }
        public Uri Url { get; set; }
    }

    public class JSONQuery
    {
        //public string QueryType { get; set; }
        //public string QueryResultType { get; set; }
        //public string AsOf { get; set; }
        //public JSONQueryColumn[] Columns { get; set; }
        public JSONQueryItem[] WorkItems { get; set; }
    }
}
