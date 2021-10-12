﻿using System;
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

        public static readonly string BUG_DATABASE_CHANGES_METRIC_KEY = "vcsparser_bugdatabase_numchanges";

        public static readonly string BUG_DATABASE_LINES_CHANGED_METRIC_KEY = "vcsparser_bugdatabase_lineschanged";

        public static readonly string AUTHORS_DATA = "vcsparser_authors_data";

        private IEnvironment environment;
        
        public MeasureConverterListBuilder(IEnvironment environment)
        {
            this.environment = environment;
        }

        public List<IMeasureConverter> Build(ISonarGenericMetrics a)
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

            CreateConvertersForData(result, a);
            return result;
        }

        private DateTime getEndDate(ISonarGenericMetrics a)
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

        private void CreateMeasureConvertersForPeriod(List<IMeasureConverter> result, DateTime startDate, DateTime endDate, string metricKeySuffix, string suffixLongDescription, ISonarGenericMetrics a)
        {
            var metricChanges = CreateMetric(CHANGES_METRIC_KEY + metricKeySuffix,
                "Number of changes (" + suffixLongDescription + ")", "Number of changes (" + suffixLongDescription + ")");
            var metricLinesChanged = CreateMetric(LINES_CHANGED_METRIC_KEY + metricKeySuffix,
                "Lines changed (" + suffixLongDescription + ")", "Lines changed (" + suffixLongDescription + ")");
            var metricChangesWithFixes = CreateMetric(CHANGES_FIXES_METRIC_KEY + metricKeySuffix,
                "Number of changes in fixes (" + suffixLongDescription + ")", "Number of changes in fixes (" + suffixLongDescription + ")");
            var metricLinesChangedWithFixes = CreateMetric(LINES_CHANGED_FIXES_METRIC_KEY + metricKeySuffix,
                "Lines changed in fixes (" + suffixLongDescription + ")", "Lines changed in fixes (" + suffixLongDescription + ")");
            var metricChangesBugDatabase = CreateMetric(BUG_DATABASE_CHANGES_METRIC_KEY + metricKeySuffix,
              "Number of changes in bug fixes (" + suffixLongDescription + ")", "Number of changes in bug fixes (" + suffixLongDescription + ")");
            var metricChangesWithFixesBugDatabase = CreateMetric(BUG_DATABASE_LINES_CHANGED_METRIC_KEY + metricKeySuffix,
               "Lines changed in bug fixes (" + suffixLongDescription + ")", "Lines changed in bug fixes (" + suffixLongDescription + ")");

            result.Add(new MeasureConverter<int>(startDate, endDate, metricChanges, new NumberOfChangesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricLinesChanged, new LinesChangedMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricChangesWithFixes, new NumberOfChangesWithFixesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricLinesChangedWithFixes, new LinesChangedWithFixesMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricChangesBugDatabase, new NumberOfChangesInFixesBugDatabaseMeasureAggregator(), a.FilePrefixToRemove));
            result.Add(new MeasureConverter<int>(startDate, endDate, metricChangesWithFixesBugDatabase, new LinesChangedInFixesBugDatabaseMeasureAggregator(), a.FilePrefixToRemove));
        }

        private void CreateConvertersForData(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var metricChanges = CreateMetric(AUTHORS_DATA, "Authors Data", "Authors Data", "DATA");

            result.Add(new MeasureConverterRaw<List<AuthorsData>>(metricChanges, new AuthorsDataAggregator(), a.FilePrefixToRemove));
        }

        private void CreateConvertersFor1Day(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_1d", "1 day", a);
        }

        private void CreateConvertersFor1Year(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddYears(-1);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_1y", "1 year", a);
        }

        private void CreateConvertersFor30Days(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-31);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_30d", "30 days", a);
        }

        private void CreateConvertersFor3Months(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-3);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_3m", "3 months", a);
        }

        private void CreateConvertersFor6Months(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-1).AddMonths(-6);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_6m", "6 months", a);
        }

        private void CreateConvertersFor7Days(List<IMeasureConverter> result, ISonarGenericMetrics a)
        {
            var endDate = getEndDate(a);
            var startDate = endDate.AddDays(-8);

            CreateMeasureConvertersForPeriod(result, startDate, endDate, "_7d", "7 days", a);
        }
    }
}
