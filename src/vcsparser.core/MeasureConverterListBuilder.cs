using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class MeasureConverterListBuilder
    {
        public static readonly string CHANGES_METRIC_KEY = "vcsparser_numchanges";

        public static readonly string LINES_CHANGED_METRIC_KEY = "vcsparser_lineschanged";

        private IEnvironment environment;
        
        public MeasureConverterListBuilder(IEnvironment environment)
        {
            this.environment = environment;
        }

        public List<IMeasureConverter> Build(SonarGenericMetricsCommandLineArgs a)
        {
            List<IMeasureConverter> result = new List<IMeasureConverter>();
            if (a.Generate1Day.ToLower() == "true")
                CreateConvertersFor1Day(result, a);
            if (a.Generate1Year.ToLower() == "true")
                CreateConvertersFor1Year(result, a);
            if (a.Generate30Days.ToLower() == "true")
                CreateConvertersFor30Days(result, a);
            if (a.Generate3Months.ToLower() == "true")
                CreateConvertersFor3Months(result, a);
            if (a.Generate6Months.ToLower() == "true")
                CreateConvertersFor6Months(result, a);
            if (a.Generate7Days.ToLower() == "true")
                CreateConvertersFor7Days(result, a);
            return result;
        }

        private DateTime getEndDate(SonarGenericMetricsCommandLineArgs a)
        {
            DateTime endDate;
            if (a.EndDate == null)
                endDate = environment.GetCurrentDateTime();
            else
                endDate = a.EndDate.Value;

            return new DateTime(endDate.Year, endDate.Month, endDate.Day);
        }

        private void CreateConvertersFor1Day(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_1d",
                Name = "Number of changes (1 day)",
                Description = "Number of changes (1 day)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_1d",
                Name = "Lines changed (1 day)",
                Description = "Lines changed (1 day)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }

        private void CreateConvertersFor1Year(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddYears(-1);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_1y",
                Name = "Number of changes (1 year)",
                Description = "Number of changes (1 year)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_1y",
                Name = "Lines changed (1 year)",
                Description = "Lines changed (1 year)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }

        private void CreateConvertersFor30Days(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-31);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_30d",
                Name = "Number of changes (30 days)",
                Description = "Number of changes (30 days)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_30d",
                Name = "Lines changed (30 days)",
                Description = "Lines changed (30 days)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }

        private void CreateConvertersFor3Months(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-3);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_3m",
                Name = "Number of changes (3 months)",
                Description = "Number of changes (3 months)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_3m",
                Name = "Lines changed (3 months)",
                Description = "Lines changed (3 months)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }

        private void CreateConvertersFor6Months(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-6);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_6m",
                Name = "Number of changes (6 months)",
                Description = "Number of changes (6 months)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_6m",
                Name = "Lines changed (6 months)",
                Description = "Lines changed (6 months)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }

        private void CreateConvertersFor7Days(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-8);

            var metricChanges = new Metric()
            {
                MetricKey = CHANGES_METRIC_KEY + "_7d",
                Name = "Number of changes (7 days)",
                Description = "Number of changes (7 days)"
            };

            var metricLinesChanged = new Metric()
            {
                MetricKey = LINES_CHANGED_METRIC_KEY + "_7d",
                Name = "Lines changed (7 days)",
                Description = "Lines changed (7 days)"
            };

            result.Add(new MeasureConverter(startDate, endDate, metricChanges, MeasureConverterType.NumberOfChanges, a.FilePrefixToRemove));
            result.Add(new MeasureConverter(startDate, endDate, metricLinesChanged, MeasureConverterType.LinesChanged, a.FilePrefixToRemove));
        }
    }
}
