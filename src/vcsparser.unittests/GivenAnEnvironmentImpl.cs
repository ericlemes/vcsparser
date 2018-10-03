using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAnEnvironmentImpl
    {
        [Fact]
        public void WhenGettingCurrentDateAndTimeShouldNotThrow()
        {
            var environment = new EnvironmentImpl();
            environment.GetCurrentDateTime();
        }
    }
}
