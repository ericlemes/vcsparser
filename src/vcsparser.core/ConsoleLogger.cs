using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class ConsoleLogger : ILogger
    {
        public void LogToConsole(string message)
        {
            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + ": " + message);
        }
    }
}
