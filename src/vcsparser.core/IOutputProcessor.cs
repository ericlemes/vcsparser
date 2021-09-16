using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public interface IOutputProcessor
    {
        void ProcessOutput<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;
    }

    public interface IOutputJson
    {
        DateTime OccurrenceDate { get; }

        string FileName { get; }
    }
}
