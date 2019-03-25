using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface IMeasureAggregator<T>
    {
        bool HasValue(DailyCodeChurn dailyCodeChurn);
        T GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn);
        T GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<T> existingMeasure);
    }

    public interface IMeasureAggregatorProject<T> : IMeasureAggregator<T>
    {
        T GetValueForProjectMeasure(DailyCodeChurn dailyCodeChurn);
    }
}
