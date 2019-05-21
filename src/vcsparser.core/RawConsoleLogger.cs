using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class RawConsoleLogger : ILogger
    {
        public void LogToConsole(string message)
        {
            Console.WriteLine(message);
        }
    }
}
