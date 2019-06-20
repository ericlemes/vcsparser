using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class AuthorsDataAggregator : IMeasureAggregator<List<AuthorsData>>
    {
        public List<AuthorsData> GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<List<AuthorsData>> existingMeasure)
        {
            existingMeasure.Value.Add(CreateAuthorsData(dailyCodeChurn));
            return existingMeasure.Value;
        }

        public List<AuthorsData> GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            var value = new List<AuthorsData>
            {
                CreateAuthorsData(dailyCodeChurn)
            };
            return value;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return (dailyCodeChurn.Authors.Count > 0);
        }

        private AuthorsData CreateAuthorsData(DailyCodeChurn dailyCodeChurn)
        {
            return new AuthorsData
            {
                Authors = dailyCodeChurn.Authors,
                Timestamp = dailyCodeChurn.GetDateTimeAsDateTime()
            };
        }
    }
}
