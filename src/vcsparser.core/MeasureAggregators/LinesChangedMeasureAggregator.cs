using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesChangedMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
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
