using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class NumberOfAuthorsMeasureAggregator : IMeasureAggregator
    {        
        private Dictionary<string, bool> currentUniqueAuthors = new Dictionary<string, bool>();

        public int GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure existingMeasure)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return currentUniqueAuthors.Count();
        }

        public int GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            UpdateCurrentUniqueAuthors(dailyCodeChurn);

            return currentUniqueAuthors.Count;
        }

        private void UpdateCurrentUniqueAuthors(DailyCodeChurn dailyCodeChurn)
        {
            foreach (var a in dailyCodeChurn.Authors)
            {
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
