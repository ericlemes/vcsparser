using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class MeasureConverterRaw<T> : IMeasureConverter
    {
        private readonly Metric metric;
        private string filePrefixToRemove;
        private readonly bool projectMeasure;

        private bool processedMetric = false;

        public Metric Metric {
            get { return metric; }
        }

        private IMeasureAggregator<T> measureAggregator;

        public IMeasureAggregator<T> MeasureAggregator {
            get { return this.measureAggregator; }
        }

        public DateTime StartDate => throw new InvalidOperationException();
        public DateTime EndDate => throw new InvalidOperationException();

        public MeasureConverterRaw(Metric metric, IMeasureAggregator<T> measureAggregator, string filePrefixToRemove)
        {
            this.metric = metric;
            this.measureAggregator = measureAggregator;
            this.filePrefixToRemove = filePrefixToRemove;
            this.projectMeasure = measureAggregator is IMeasureAggregatorProject<T>;
        }

        public void ProcessFileMeasure(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson)
        {
            if (!ValidDailyCodeChurn(dailyCodeChurn))
                return;

            ProcessMetric(sonarMeasuresJson);

            var fileName = ProcessFileName(dailyCodeChurn.FileName, filePrefixToRemove);

            var existingMeasureRaw = sonarMeasuresJson.FindRawMeasure(metric.MetricKey, fileName) as Measure<T>;

            if (existingMeasureRaw == null)
            {
                sonarMeasuresJson.AddRawMeasure(new Measure<T>()
                {
                    MetricKey = this.metric.MetricKey,
                    Value = measureAggregator.GetValueForNewMeasure(dailyCodeChurn),
                    File = fileName
                });
            }
            else
            {
                existingMeasureRaw.Value = measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, existingMeasureRaw);
            }
        }

        public void ProcessProjectMeasure(SonarMeasuresJson sonarMeasuresJson)
        {
            if (!projectMeasure)
                return;

            ProcessMetric(sonarMeasuresJson);

            var existingMeasureProject = sonarMeasuresJson.FindProjectMeasure(metric.MetricKey) as Measure<T>;
            var projectMeasureAggregator = measureAggregator as IMeasureAggregatorProject<T>;

            if (existingMeasureProject == null)
            {
                sonarMeasuresJson.AddProjectMeasure(new Measure<T>()
                {
                    MetricKey = this.metric.MetricKey,
                    Value = projectMeasureAggregator.GetValueForProjectMeasure()
                });
            }
            else
            {
                existingMeasureProject.Value = projectMeasureAggregator.GetValueForProjectMeasure();
            }
        }

        private bool ValidDailyCodeChurn(DailyCodeChurn dailyCodeChurn)
        {
            if (!measureAggregator.HasValue(dailyCodeChurn))
                return false;

            return true;
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
