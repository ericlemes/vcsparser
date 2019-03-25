using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class MeasureConverter<T> : IMeasureConverter
    {
        private DateTime startDate;
        private DateTime endDate;
        private Metric metric;
        private bool processedMetric = false;
        public Metric Metric {
            get { return metric; }
        }

        public DateTime StartDate {
            get { return startDate; }
        }

        public DateTime EndDate {
            get { return endDate; }
        }

        private IMeasureAggregator<T> measureAggregator;

        public IMeasureAggregator<T> MeasureAggregator {
            get { return this.measureAggregator; }
        }

        private string filePrefixToRemove;

        public MeasureConverter(DateTime startDate, DateTime endDate, Metric metric, IMeasureAggregator<T> measureAggregator, string filePrefixToRemove)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.metric = metric;
            this.measureAggregator = measureAggregator;
            this.filePrefixToRemove = filePrefixToRemove;
        }

        public void Process(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson)
        {
            if (dailyCodeChurn.GetDateTimeAsDateTime() < startDate || dailyCodeChurn.GetDateTimeAsDateTime() > endDate)
                return;

            ProcessMetric(sonarMeasuresJson);

            if (!measureAggregator.HasValue(dailyCodeChurn))
                return;

            var fileName = ProcessFileName(dailyCodeChurn.FileName, filePrefixToRemove);

            var existingMeasure = sonarMeasuresJson.FindMeasure(metric.MetricKey, fileName) as Measure<T>;
            if (existingMeasure == null)
            {
                sonarMeasuresJson.AddMeasure(new Measure<T>()
                {
                    MetricKey = this.metric.MetricKey,
                    File = fileName,
                    Value = measureAggregator.GetValueForNewMeasure(dailyCodeChurn)
                });
            }
            else
            {
                existingMeasure.Value = measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, existingMeasure);
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
