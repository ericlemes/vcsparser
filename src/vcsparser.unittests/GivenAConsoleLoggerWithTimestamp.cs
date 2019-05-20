using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAConsoleLoggerWithTimestamp
    {
        [Fact]
        public void WhenLoggingToConsoleShouldNotThrow()
        {
            var logger = new ConsoleLoggerWithTimestamp();
            logger.LogToConsole("blah");
        }
    }
}
