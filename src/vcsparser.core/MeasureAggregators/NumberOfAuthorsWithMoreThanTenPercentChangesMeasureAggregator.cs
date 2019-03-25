using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator : IMeasureAggregatorProject<int>
    {
        private readonly int THRESHOLD = 10;

        private Dictionary<string, Dictionary<string, int>> currentUniqueAuthorsPerFile = new Dictionary<string, Dictionary<string, int>>();

        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);
            return CalculateNumberOfAuthorsOverThreshold(dailyCodeChurn, THRESHOLD);
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return CalculateNumberOfAuthorsOverThreshold(dailyCodeChurn, THRESHOLD);
        }

        public int GetValueForProjectMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return CalculateNumberOfAuthorsOverThreshold(dailyCodeChurn, THRESHOLD);
        }

        private void UpdateCurrentUniqueAuthors(DailyCodeChurn dailyCodeChurn)
        {
            if (!currentUniqueAuthorsPerFile.ContainsKey(dailyCodeChurn.FileName))
                currentUniqueAuthorsPerFile.Add(dailyCodeChurn.FileName, new Dictionary<string, int>());

            foreach (var a in dailyCodeChurn.Authors)
            {
                var currentUniqueAuthors = currentUniqueAuthorsPerFile[dailyCodeChurn.FileName];
                if (!currentUniqueAuthors.ContainsKey(a.Author.ToLower()))
                    currentUniqueAuthors.Add(a.Author.ToLower(), a.NumberOfChanges);
                else
                    currentUniqueAuthors[a.Author.ToLower()] += a.NumberOfChanges;
            }
        }

        private int CalculateNumberOfAuthorsOverThreshold(DailyCodeChurn dailyCodeChurn, int threshold)
        {
            var currentUniqueAuthors = currentUniqueAuthorsPerFile[dailyCodeChurn.FileName];

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
