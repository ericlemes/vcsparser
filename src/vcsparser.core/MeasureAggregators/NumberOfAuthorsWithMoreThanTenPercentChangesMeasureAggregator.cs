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

        public int GetValueForProjectMeasure()
        {
            List<string> uniqueAuthors = new List<string>();
            foreach (var file in currentUniqueAuthorsPerFile.Keys)
            {
                foreach (var author in CalculateListOfAuthorsOverThreshold(file, THRESHOLD))
                    uniqueAuthors.Add(author);
            }
            return uniqueAuthors.Distinct().Count();
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
            return CalculateListOfAuthorsOverThreshold(dailyCodeChurn.FileName, threshold).Count();
        }

        private IEnumerable<string> CalculateListOfAuthorsOverThreshold(string fileName, int threshold)
        {
            List<string> authors = new List<string>();
            var currentUniqueAuthors = currentUniqueAuthorsPerFile[fileName];
            var total = currentUniqueAuthors.Sum(p => p.Value);
            foreach (var p in currentUniqueAuthors)
            {
                double perc = (((double)p.Value) * 100f) / ((double)total);
                if (perc > threshold)
                    authors.Add(p.Key);
            }
            return authors;
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return true;
        }
    }
}
