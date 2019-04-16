using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface ITimeKeeper
    {
        bool IsCompleted { get; }
        Action IntervalAction { get; set; }
        void Start();
        void Cancel();
    }

    public class TimeKeeper : ITimeKeeper
    {
        private readonly CancellationTokenSource cancellationTokenSource;
        private readonly Task task;

        public bool IsCompleted { get => task.IsCompleted; }

        public Action IntervalAction { get; set; }

        public TimeKeeper(TimeSpan delay) : this(delay, () => { }) { }
        public TimeKeeper(TimeSpan delay, Action intervalAction)
        {
            cancellationTokenSource = new CancellationTokenSource();
            IntervalAction = intervalAction;
            task = new Task(async () =>
            {
                while (!cancellationTokenSource.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(delay, cancellationTokenSource.Token);
                        IntervalAction();
                    }
                    catch (Exception) { }
                }
            }, cancellationTokenSource.Token);
        }

        public void Start() => task.Start();

        public void Cancel() => cancellationTokenSource.Cancel();
    }
}
