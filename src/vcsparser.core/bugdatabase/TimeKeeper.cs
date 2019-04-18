using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public class TimeKeeper : ITimeKeeper
    {
        public TimeSpan Delay { get; set; }
        public Action IntervalAction { get; set; }
        public bool IsCompleted { get => task.IsCompleted; }

        private readonly CancellationTokenSource cancellationTokenSource;
        private Task task;

        public TimeKeeper(TimeSpan delay) : this(delay, () => { }) { }
        public TimeKeeper(TimeSpan delay, Action intervalAction)
        {
            cancellationTokenSource = new CancellationTokenSource();
            IntervalAction = intervalAction;
            Delay = delay;
        }

        private void RunInterval()
        {
            try
            {
                Task.Delay(Delay, cancellationTokenSource.Token).Wait();
                IntervalAction();
            }
            catch (TaskCanceledException)
            {
                return;
            }
        }

        public Task Start()
        {
            return task = Task.Run(() =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                    RunInterval();
            }, cancellationTokenSource.Token);
        }

        public void Cancel() => cancellationTokenSource.Cancel();
    }
}
