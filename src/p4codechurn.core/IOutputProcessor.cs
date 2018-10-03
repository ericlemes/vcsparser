using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public interface IOutputProcessor
    {
        void ProcessOutput(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict);
    }
}
