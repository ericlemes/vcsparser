using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesFixedOverChangedMetricsMeasureAggregator : IMeasureAggregatorProject<double>
    {
        private Dictionary<string, List<DailyCodeChurn>> currentChanges = new Dictionary<string, List<DailyCodeChurn>>();

        public double GetValueForExistingMeasure(DailyCodeChurn dailyCodeChurn, Measure<double> existingMeasure)
        {
            UpdateCurrentChanges(dailyCodeChurn);
            return CalculateCurrentChange(dailyCodeChurn.FileName);
        }

        public double GetValueForNewMeasure(DailyCodeChurn dailyCodeChurn)
        {
            UpdateCurrentChanges(dailyCodeChurn);
            return CalculateCurrentChange(dailyCodeChurn.FileName);
        }

        public double GetValueForProjectMeasure()
        {
            return CalculateCurrent();
        }

        private double CalculateCurrentChange(string fileName)
        {
            int totalLinesChangedWithFixes = 0;
            int totalLinesChanged = 0;

            foreach (var churn in currentChanges[fileName])
            {
                totalLinesChangedWithFixes += churn.TotalLinesChangedWithFixes;
                totalLinesChanged += churn.TotalLinesChanged;
            }

            return (totalLinesChangedWithFixes * 100.0) / totalLinesChanged;
        }

        private double CalculateCurrent()
        {
            int totalLinesChangedWithFixes = 0;
            int totalLinesChanged = 0;

            foreach (string fileName in currentChanges.Keys)
            {
                foreach (var churn in currentChanges[fileName])
                {
                    totalLinesChangedWithFixes += churn.TotalLinesChangedWithFixes;
                    totalLinesChanged += churn.TotalLinesChanged;
                }
            }

            if (totalLinesChanged == 0) return 0;
            return (totalLinesChangedWithFixes * 100.0) / totalLinesChanged;
        }

        private void UpdateCurrentChanges(DailyCodeChurn dailyCodeChurn)
        {
            if (!currentChanges.ContainsKey(dailyCodeChurn.FileName))
                currentChanges.Add(dailyCodeChurn.FileName, new List<DailyCodeChurn>());
            
            currentChanges[dailyCodeChurn.FileName].Add(dailyCodeChurn);
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChanged > 0;
        }
    }
}
