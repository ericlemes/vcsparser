using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenATimeKeeper
    {
        private ITimeKeeper timeKeeper;

        [Fact]
        public void WhenStartCalledAfterCancelThenThrowInvalidOperationException()
        {
            Mock<Action> someAction = new Mock<Action>();

            timeKeeper = new TimeKeeper(TimeSpan.Zero, someAction.Object);
            timeKeeper.Cancel();
            Action start = () => timeKeeper.Start();

            var exception = Assert.Throws<InvalidOperationException>(start);
            Assert.Equal("Start may not be called on a task that has completed.", exception.Message);
        }

        [Fact]
        public void WhenStartThenRunThread()
        {
            Mock<Action> someAction = new Mock<Action>();

            var waitHandle = new ManualResetEvent(false);
            Action action = () =>
            {
                someAction.Object();
                waitHandle.Set();
            };

            timeKeeper = new TimeKeeper(TimeSpan.Zero, action);
            timeKeeper.Start();

            waitHandle.WaitOne(TimeSpan.FromSeconds(1));
            timeKeeper.Cancel();

            someAction.Verify(a => a(), Times.AtLeastOnce);
        }

        [Fact]
        public void WhenCancelThenStopTask()
        {
            var waitHandle = new ManualResetEvent(false);

            timeKeeper = new TimeKeeper(TimeSpan.Zero, () => waitHandle.Set());
            timeKeeper.Start();
            timeKeeper.Cancel();

            waitHandle.WaitOne(TimeSpan.FromSeconds(1));

            Assert.True(timeKeeper.IsCompleted);
        }
    }
}
