using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesChangedWithFixesMeasureAggregator : IMeasureAggregator
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure existingMeasure)
        {
            return dailyCodeChurn.TotalLinesChangedWithFixes + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChangedWithFixes;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChangedWithFixes > 0;
        }
    }
}
