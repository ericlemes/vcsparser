using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator : IMeasureAggregator
    {
        private readonly int THRESHOLD = 10;

        private Dictionary<string, int> currentUniqueAuthors = new Dictionary<string, int>();

        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure existingMeasure)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);
            return CalculateNumberOfAuthorsOverThreshold(THRESHOLD);
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return CalculateNumberOfAuthorsOverThreshold(THRESHOLD);
        }

        private void UpdateCurrentUniqueAuthors(DailyCodeChurn dailyCodeChurn)
        {
            foreach (var a in dailyCodeChurn.Authors)
            {
                if (!currentUniqueAuthors.ContainsKey(a.Author.ToLower()))
                    currentUniqueAuthors.Add(a.Author.ToLower(), a.NumberOfChanges);
                else
                    currentUniqueAuthors[a.Author.ToLower()] += a.NumberOfChanges;
            }
        }

        private int CalculateNumberOfAuthorsOverThreshold(int threshold)
        {
            var total = currentUniqueAuthors.Sum(p => p.Value);
            var countOverThreshold = 0;
            foreach (var p in currentUniqueAuthors)
            {
                double perc = (((double)p.Value) * 100f) / ((double)total);
                if (perc > threshold)
                    countOverThreshold++;
            }
            return countOverThreshold;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return true;
        }
    }
}
