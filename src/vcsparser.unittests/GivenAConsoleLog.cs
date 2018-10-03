using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAConsoleLog
    {
        [Fact]
        public void WhenLoggingToConsoleShouldNotThrow()
        {
            var logger = new ConsoleLogger();
            logger.LogToConsole("blah");
        }
    }
}
