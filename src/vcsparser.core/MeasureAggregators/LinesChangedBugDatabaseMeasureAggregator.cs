using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesChangedBugDatabaseMeasureAggregator : IMeasureAggregator<int>
    {
        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            return dailyCodeChurn.BugDatabse.TotalLinesChanged + existingMeasure.Value;
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.BugDatabse.TotalLinesChanged;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            if (dailyCodeChurn.BugDatabse == null) return false;
            return dailyCodeChurn.BugDatabse.TotalLinesChanged > 0;
        }
    }
}
