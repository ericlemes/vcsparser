using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public enum MeasureConverterType
    {
        LinesChanged,
        NumberOfChanges,
        LinesChangedWithFixes,
        NumberOfChangesWithFixes
    }

    public class MeasureConverter : IMeasureConverter
    {
        private DateTime startDate;
        private DateTime endDate;
        private Metric metric;
        private MeasureConverterType type;
        private bool processedMetric = false;

        public Metric Metric
        {
            get { return metric; }
        }

        public DateTime StartDate
        {
            get { return startDate; }
        }

        public DateTime EndDate
        {
            get { return endDate; }
        }

        private string filePrefixToRemove;

        public MeasureConverter(DateTime startDate, DateTime endDate, Metric metric, MeasureConverterType type, string filePrefixToRemove)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.metric = metric;
            this.type = type;
            this.filePrefixToRemove = filePrefixToRemove;
        }

        public void Process(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson)
        {
            if (dailyCodeChurn.GetDateTimeAsDateTime() < startDate || dailyCodeChurn.GetDateTimeAsDateTime() > endDate)
                return;

            ProcessMetric(sonarMeasuresJson);

            var value = GetValue(dailyCodeChurn);
            if (value <= 0)
                return;

            var fileName = ProcessFileName(dailyCodeChurn.FileName, filePrefixToRemove);

            var existingMeasure = sonarMeasuresJson.FindMeasure(metric.MetricKey, fileName);
            if (existingMeasure == null)
            {
                sonarMeasuresJson.AddMeasure(new Measure()
                {
                    MetricKey = this.metric.MetricKey,
                    File = fileName,
                    Value = value
                });
            }
            else
            {
                existingMeasure.Value += value;
            }
        }

        private int GetValue(DailyCodeChurn dailyCodeChurn)
        {
            switch (this.type) {
                case MeasureConverterType.LinesChanged:
                    return dailyCodeChurn.TotalLinesChanged;                    
                case MeasureConverterType.NumberOfChanges:
                    return dailyCodeChurn.NumberOfChanges;
                case MeasureConverterType.LinesChangedWithFixes:
                    return dailyCodeChurn.TotalLinesChangedWithFixes;
                default:
                    return dailyCodeChurn.NumberOfChangesWithFixes;
            }
        }

        private string ProcessFileName(string fileName, string filePrefixToRemove)
        {
            if (filePrefixToRemove == null)
                return fileName;

            if (fileName.StartsWith(filePrefixToRemove))
                return fileName.Substring(filePrefixToRemove.Length);

            return fileName;
        }

        private void ProcessMetric(SonarMeasuresJson sonarMeasuresJson)
        {
            if (processedMetric)
                return;

            sonarMeasuresJson.Metrics.Add(metric);

            processedMetric = true;
        }
    }
}
