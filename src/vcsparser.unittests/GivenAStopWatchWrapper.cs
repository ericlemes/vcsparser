using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAStopWatchWrapper
    {
        [Fact]
        public void WhenDoingBasicOperationsShouldNotThrow()
        {
            var stopWatch = new StopWatchWrapper();
            stopWatch.Restart();
            stopWatch.Stop();
            stopWatch.TotalSecondsElapsed();
        }
    }
}
