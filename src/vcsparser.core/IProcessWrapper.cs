using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IProcessWrapper
    {
        Stream Invoke(string executable, string arguments);
    }
}
