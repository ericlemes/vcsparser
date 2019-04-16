using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfChangesBugDatabaseMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            return dailyCodeChurn.BugDatabse.NumberOfChanges + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.BugDatabse.NumberOfChanges;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            if (dailyCodeChurn.BugDatabse == null) return false;
            return dailyCodeChurn.BugDatabse.NumberOfChanges > 0;
        }
    }
}
