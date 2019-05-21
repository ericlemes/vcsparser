using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenARawConsoleLogger
    {
        [Fact]
        public void WhenLoggingToConsoleShouldNotThrow()
        {
            var logger = new RawConsoleLogger();
            logger.LogToConsole("blah");
        }
    }
}
