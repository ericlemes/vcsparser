using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.bugdatabase.azuredevops
{
    public class JSONQueryItem
    {
        public string Id { get; set; }
        public Uri Url { get; set; }
    }

    public class JSONQuery
    {
        public JSONQueryItem[] WorkItems { get; set; }
    }
}
