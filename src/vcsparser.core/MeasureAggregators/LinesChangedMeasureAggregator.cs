using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesChangedMeasureAggregator : IMeasureAggregator
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure existingMeasure)
        {
            return dailyCodeChurn.TotalLinesChanged + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChanged;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChanged > 0;
        }
    }
}
