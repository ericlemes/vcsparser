using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public interface ICommandLineParser
    {
        Tuple<string, string> ParseCommandLine(string commandLine);
    }
}
