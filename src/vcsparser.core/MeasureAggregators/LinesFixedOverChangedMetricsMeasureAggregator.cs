using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.MeasureAggregators
{
    public class LinesFixedOverChangedMetricsMeasureAggregator : IMeasureAggregatorProject<double>
    {
        private Dictionary<string, List<int[]>> currentChanges = new Dictionary<string, List<int[]>>();

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

        public double GetValueForProjectMeasure(DailyCodeChurn dailyCodeChurn)
        {
            return CalculateCurrent();
        }

        private double CalculateCurrentChange(string fileName)
        {
            int totalLinesChangedWithFixes = 0;
            int totalLinesChanged = 0;

            foreach (var a in currentChanges[fileName])
            {
                totalLinesChangedWithFixes += a[0];
                totalLinesChanged += a[1];
            }

            return (totalLinesChangedWithFixes * 100.0) / totalLinesChanged;
        }

        private double CalculateCurrent()
        {
            int totalLinesChangedWithFixes = 0;
            int totalLinesChanged = 0;

            foreach (string fileName in currentChanges.Keys)
            {
                foreach (var a in currentChanges[fileName])
                {
                    totalLinesChangedWithFixes += a[0];
                    totalLinesChanged += a[1];
                }
            }

            return (totalLinesChangedWithFixes * 100.0) / totalLinesChanged;
        }

        private void UpdateCurrentChanges(DailyCodeChurn dailyCodeChurn)
        {
            if (!currentChanges.ContainsKey(dailyCodeChurn.FileName))
                currentChanges.Add(dailyCodeChurn.FileName, new List<int[]>());

            int[] changes = new int[2] { dailyCodeChurn.TotalLinesChangedWithFixes, dailyCodeChurn.TotalLinesChanged };
            currentChanges[dailyCodeChurn.FileName].Add(changes);
        }

        public bool HasValue(DailyCodeChurn dailyCodeChurn)
        {
            return dailyCodeChurn.TotalLinesChanged > 0;
        }
    }
}
