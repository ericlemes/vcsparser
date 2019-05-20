using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IJsonListParser<T> where T : IOutputJson
    {
        IList<T> ParseFile(string fileName);
    }
}
