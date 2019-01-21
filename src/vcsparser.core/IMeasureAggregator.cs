using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IMeasureAggregator
    {
        bool HasValue(DailyCodeChurn dailyCodeChurn);
        int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn);
        int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure existingMeasure);
    }
}
