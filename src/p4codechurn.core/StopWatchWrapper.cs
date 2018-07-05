using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class StopWatchWrapper : IStopWatch
    {
        private Stopwatch stopwatch;
        public StopWatchWrapper()
        {
            this.stopwatch = new Stopwatch();
        }
        public void Restart()
        {
            this.stopwatch.Restart();
        }

        public void Stop()
        {
            this.stopwatch.Stop();
        }

        public int TotalSecondsElapsed()
        {
            return (int)this.stopwatch.Elapsed.TotalSeconds;
        }
    }
}
