using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfChangesMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            return dailyCodeChurn.NumberOfChanges + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.NumberOfChanges;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return (dailyCodeChurn.NumberOfChanges > 0);
        }
    }
}
