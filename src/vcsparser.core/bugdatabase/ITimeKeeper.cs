using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface ITimeKeeper
    {
        TimeSpan Delay { get; set; }
        Action IntervalAction { get; set; }
        bool IsCompleted { get; }
        Task Start();
        void Cancel();
    }
}
