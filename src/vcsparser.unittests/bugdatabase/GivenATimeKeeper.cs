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
        public void WhenStartCalledAfterCancelThenThrowTaskCanceledException()
        {
            Mock<Action> someAction = new Mock<Action>();

            timeKeeper = new TimeKeeper(TimeSpan.Zero, someAction.Object);
            timeKeeper.Cancel();
            Action start = () => timeKeeper.Start().Wait();

            var aggregateException = Assert.Throws<AggregateException>(start);
            Assert.IsType<TaskCanceledException>(aggregateException.InnerException);
            someAction.Verify(a => a(), Times.Never);
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

        [Fact]
        public void WhenActionThrowsTaskCancledThenBreakTask()
        {
            var waitHandle = new ManualResetEvent(false);
            timeKeeper = new TimeKeeper(TimeSpan.Zero, () => timeKeeper.Cancel());
            timeKeeper.Start().Wait();
            Assert.True(timeKeeper.IsCompleted);
        }

        [Fact]
        public void WhenActionThrowsThenBreakTask()
        {
            timeKeeper = new TimeKeeper(TimeSpan.Zero, () => throw new Exception("Some Exception!"));
            Action start = () => timeKeeper.Start().Wait();

            var aggregateException = Assert.Throws<AggregateException>(start);
            var exception = Assert.IsType<Exception>(aggregateException.InnerException);
            Assert.Equal("Some Exception!", exception.Message);
        }

        [Fact]
        public void WhenActionThrowsTaskCanceledExceptionThenDoNothing()
        {
            Mock<Action> someAction = new Mock<Action>();
            someAction.SetupSequence(a => a()).Throws(new TaskCanceledException("Some Exceptiom!")).Pass();

            timeKeeper = new TimeKeeper(TimeSpan.Zero, () =>
            {
                someAction.Object();
                timeKeeper.Cancel();
            });
            timeKeeper.Start().Wait();

            Assert.True(timeKeeper.IsCompleted);
            someAction.Verify(a => a(), Times.Exactly(2));
        }
    }
}
