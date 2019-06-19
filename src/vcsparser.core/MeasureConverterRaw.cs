using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class MeasureConverterRaw<T> : MeasureConverter<T>
    {
        public MeasureConverterRaw(Metric metric, IMeasureAggregator<T> measureAggregator, string filePrefixToRemove)
            : base(DateTime.Today, DateTime.Today, metric, measureAggregator, filePrefixToRemove)
        { }

        public override void ProcessFileMeasure(DailyCodeChurn dailyCodeChurn, SonarMeasuresJson sonarMeasuresJson)
        {
            if (!ValidDailyCodeChurn(dailyCodeChurn))
                return;

            ProcessMetric(sonarMeasuresJson);

            var fileName = ProcessFileName(dailyCodeChurn.FileName, filePrefixToRemove);

            var existingMeasureFile = sonarMeasuresJson.FindRawMeasure(metric.MetricKey, fileName) as Measure<T>;

            if (existingMeasureFile == null)
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
                existingMeasureFile.Value = measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, existingMeasureFile);
            }
        }

        public override void ProcessProjectMeasure(SonarMeasuresJson _)
        {}

        protected override bool ValidDailyCodeChurn(DailyCodeChurn dailyCodeChurn)
        {
            if (!measureAggregator.HasValue(dailyCodeChurn))
                return false;

            return true;
        }
    }
}
