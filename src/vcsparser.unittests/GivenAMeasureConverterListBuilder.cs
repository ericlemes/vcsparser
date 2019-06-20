using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.unittests
{
    public class GivenAMeasureConverterListBuilder
    {
        private Mock<IEnvironment> environmentMock;

        private MeasureConverterListBuilder builder;       

        public GivenAMeasureConverterListBuilder()
        {
            environmentMock = new Mock<IEnvironment>();
            environmentMock.Setup(m => m.GetCurrentDateTime()).Returns(new DateTime(2018, 09, 17, 15, 20, 00));

            builder = new MeasureConverterListBuilder(environmentMock.Object);
        }

        [Fact]
        public void WhenBuildingShouldCreateMeasureConverters()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate1Day = "true";
            args.Generate1Year = "true";
            args.Generate30Days = "true";
            args.Generate3Months = "true";
            args.Generate6Months = "true";
            args.Generate7Days = "true";            
            
            var converters = builder.Build(args);
            Assert.Equal(48, converters.Count);
        }

        [Fact]
        public void WhenBuildingWithoutEndDateShouldUseSystemDate()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = null;
            args.Generate1Day = "true";

            var converters = builder.Build(args);

            AssertAllMeasureConverters(converters, "_1d", new DateTime(2018, 9, 16, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));            
        }

        private void AssertMeasureConverter<MeasureAggregatorType, T>(List<IMeasureConverter> converters, string metricKey, DateTime startDate, DateTime endDate)
        {
            var measureConverter = (MeasureConverter<T>)converters.Where(c => c is MeasureConverter<T> && ((MeasureConverter<T>)c).Metric.MetricKey == metricKey).First();
            Assert.IsType<MeasureAggregatorType>(measureConverter.MeasureAggregator);
            Assert.Equal(startDate, measureConverter.StartDate);
            Assert.Equal(endDate, measureConverter.EndDate);
        }

        private void AssertAllMeasureConverters(List<IMeasureConverter> converters, string metricKeySuffix, DateTime startDate, DateTime endDate)
        {
            Assert.Equal(8, converters.Count);
            AssertMeasureConverter<NumberOfChangesMeasureAggregator, int>(converters, MeasureConverterListBuilder.CHANGES_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<LinesChangedMeasureAggregator, int>(converters, MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<NumberOfChangesWithFixesMeasureAggregator, int>(converters, MeasureConverterListBuilder.CHANGES_FIXES_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<LinesChangedWithFixesMeasureAggregator, int>(converters, MeasureConverterListBuilder.LINES_CHANGED_FIXES_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<NumberOfAuthorsMeasureAggregator, int>(converters, MeasureConverterListBuilder.NUM_AUTHORS + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator, int>(converters, MeasureConverterListBuilder.NUM_AUTHORS_10_PERC + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<NumberOfChangesInFixesBugDatabaseMeasureAggregator, int>(converters, MeasureConverterListBuilder.BUG_DATABASE_CHANGES_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
            AssertMeasureConverter<LinesChangedInFixesBugDatabaseMeasureAggregator, int>(converters, MeasureConverterListBuilder.BUG_DATABASE_LINES_CHANGED_METRIC_KEY + metricKeySuffix,
                startDate, endDate);
        }

        [Fact]
        public void WhenBuilding1DayShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate1Day = "true";            
            
            var converters = builder.Build(args);                                               

            AssertAllMeasureConverters(converters, "_1d", new DateTime(2018, 9, 16, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }

        [Fact]
        public void WhenBuilding1YearShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);            
            args.Generate1Year = "true";

            var converters = builder.Build(args);

            AssertAllMeasureConverters(converters, "_1y", new DateTime(2017, 9, 16, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }

        [Fact]
        public void WhenBuilding30DaysShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate30Days = "true";

            var converters = builder.Build(args);
            AssertAllMeasureConverters(converters, "_30d", new DateTime(2018, 8, 17, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }

        [Fact]
        public void WhenBuilding3MonthsShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate3Months = "true";

            var converters = builder.Build(args);
            AssertAllMeasureConverters(converters, "_3m", new DateTime(2018, 6, 16, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }

        [Fact]
        public void WhenBuilding6MonthsShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate6Months = "true";

            var converters = builder.Build(args);
            AssertAllMeasureConverters(converters, "_6m", new DateTime(2018, 3, 16, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }

        [Fact]
        public void WhenBuilding7DaysShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate7Days = "true";

            var converters = builder.Build(args);
            AssertAllMeasureConverters(converters, "_7d", new DateTime(2018, 9, 9, 00, 00, 00), new DateTime(2018, 9, 17, 00, 00, 00));
        }
    }
}
