using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public interface IOutputProcessor
    {
        void ProcessOutputSingleFile(string fileName, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict);
        void ProcessOutputMultipleFile(string filePrefix, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict);
    }
}
