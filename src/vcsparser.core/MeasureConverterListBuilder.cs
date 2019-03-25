using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class MeasureConverterListBuilder
    {
        public static readonly string CHANGES_METRIC_KEY = "vcsparser_numchanges";

        public static readonly string CHANGES_FIXES_METRIC_KEY = "vcsparser_numchanges_fixes";

        public static readonly string LINES_CHANGED_METRIC_KEY = "vcsparser_lineschanged";

        public static readonly string LINES_CHANGED_FIXES_METRIC_KEY = "vcsparser_lineschanged_fixes";

        public static readonly string NUM_AUTHORS = "vcsparser_numauthors";

        public static readonly string NUM_AUTHORS_10_PERC = "vcsparser_numauthors10perc";

        public static readonly string LINE_FIXED_OVER_CHANGED_METRIC_KEY = "vcsparser_linesfixedoverchanges";

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

        private Metric CreateMetric(string key, string name, string description, string type = "INT")
        {
            return new Metric()
            {
                MetricKey = key,
                Name = name,
                Description = description,
                Type = type
            };
        }

        private void CreateMeasureConvertersForPeriod(List<IMeasureConverter> result, DateTime startDate, DateTime endDate, string metricKeySuffix, string suffixLongDescription, SonarGenericMetricsCommandLineArgs a)
        {
            var metricChanges = CreateMetric(CHANGES_METRIC_KEY + metricKeySuffix,
                "Number of changes (" + suffixLongDescription + ")", "Number of changes (" + suffixLongDescription + ")");
            var metricLinesChanged = CreateMetric(LINES_CHANGED_METRIC_KEY + metricKeySuffix,
                "Lines changed (" + suffixLongDescription + ")", "Lines changed (" + suffixLongDescription + ")");
            var metricChangesWithFixes = CreateMetric(CHANGES_FIXES_METRIC_KEY + metricKeySuffix,
                "Number of changes in fixes (" + suffixLongDescription + ")", "Number of changes in fixes (" + suffixLongDescription + ")");
            var metricLinesChangedWithFixes = CreateMetric(LINES_CHANGED_FIXES_METRIC_KEY + metricKeySuffix,
                "Lines changed in fixes (" + suffixLongDescription + ")", "Lines changed in fixes (" + suffixLongDescription + ")");
            var metricNumAuthors = CreateMetric(NUM_AUTHORS + metricKeySuffix,
                "Number of authors (" + suffixLongDescription + ")", "Number of authors (" + suffixLongDescription + ")");
            var metricNumAuthors10Perc = CreateMetric(NUM_AUTHORS_10_PERC + metricKeySuffix,
                "Number of authors over 10% contrib (" + suffixLongDescription + ")", "Number of authors with over 10% of changes (" + suffixLongDescription + ")");
            var metricLineFixedOverChanges = CreateMetric(LINE_FIXED_OVER_CHANGED_METRIC_KEY + metricKeySuffix,
                "Percentage of lines fixed over changed (" + suffixLongDescription + ")", "Percentage of lines fixed over changed (" + suffixLongDescription + ")", "PERCENT");

            result.Add(new MeasureConverter<int>(startDate, endDate, metricChanges, new NumberOfChangesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricLinesChanged, new LinesChangedMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricChangesWithFixes, new NumberOfChangesWithFixesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricLinesChangedWithFixes, new LinesChangedWithFixesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricNumAuthors, new NumberOfAuthorsMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricNumAuthors10Perc, new NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<double>(startDate, endDate, metricLineFixedOverChanges, new LinesFixedOverChangedMetricsMeasureAggregator(), a.FilePrefixToRemove));
        }

        private void CreateConvertersFor1Day(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_1d", "1 day", a);
        }

        private void CreateConvertersFor1Year(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddYears(-1);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_1y", "1 year", a);
        }

        private void CreateConvertersFor30Days(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-31);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_30d", "30 days", a);
        }

        private void CreateConvertersFor3Months(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-3);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_3m", "3 months", a);
        }

        private void CreateConvertersFor6Months(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-6);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_6m", "6 months", a);
        }

        private void CreateConvertersFor7Days(List<IMeasureConverter> result, SonarGenericMetricsCommandLineArgs a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-8);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_7d", "7 days", a);
        }
    }
}
