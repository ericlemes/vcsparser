using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfAuthorsMeasureAggregator : IMeasureAggregatorProject<int>
    {        
        private Dictionary<string, Dictionary<string, bool>> currentUniqueAuthorsPerFile = new Dictionary<string, Dictionary<string, bool>>();

        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<int> existingMeasure)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return currentUniqueAuthorsPerFile[dailyCodeChurn.FileName].Count();
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return currentUniqueAuthorsPerFile[dailyCodeChurn.FileName].Count();
        }

        public int GetValueForProjectMeasure()
        {
            Dictionary<string, bool> uniqueAuthors = new Dictionary<string, bool>();
            foreach (var files in currentUniqueAuthorsPerFile.Values)
            {
                foreach (var author in files.Keys)
                {
                    if (!uniqueAuthors.ContainsKey(author))
                        uniqueAuthors.Add(author, true);
                }
            }
            return uniqueAuthors.Count();
        }

        private void UpdateCurrentUniqueAuthors(DailyCodeChurn dailyCodeChurn)
        {
            if (!currentUniqueAuthorsPerFile.ContainsKey(dailyCodeChurn.FileName))
                currentUniqueAuthorsPerFile.Add(dailyCodeChurn.FileName, new Dictionary<string, bool>());

            foreach (var a in dailyCodeChurn.Authors)
            {
                var currentUniqueAuthors = currentUniqueAuthorsPerFile[dailyCodeChurn.FileName];
                if (!currentUniqueAuthors.ContainsKey(a.Author.ToLower()))
                    currentUniqueAuthors.Add(a.Author.ToLower(), true);
            }
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {            
            return true;
        }
    }
}
