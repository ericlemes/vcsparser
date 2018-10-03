using Moq;
using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
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
            Assert.Equal(12, converters.Count);
        }

        [Fact]
        public void WhenBuildingWithoutEndDateShouldUseSystemDate()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = null;
            args.Generate1Day = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_1d").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_1d").First();
            Assert.Equal(new DateTime(2018, 9, 16, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 9, 16, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding1DayShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate1Day = "true";            
            
            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_1d").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY+ "_1d").First();
            Assert.Equal(new DateTime(2018, 9, 16, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 9, 16, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding1YearShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);            
            args.Generate1Year = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_1y").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_1y").First();
            Assert.Equal(new DateTime(2017, 9, 16, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2017, 9, 16, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding30DaysShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate30Days = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_30d").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_30d").First();
            Assert.Equal(new DateTime(2018, 8, 17, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 8, 17, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding3MonthsShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate3Months = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_3m").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_3m").First();
            Assert.Equal(new DateTime(2018, 6, 16, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 6, 16, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding6MonthsShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate6Months = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_6m").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_6m").First();
            Assert.Equal(new DateTime(2018, 3, 16, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 3, 16, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }

        [Fact]
        public void WhenBuilding7DaysShouldComputeDatesCorrectly()
        {
            var args = new SonarGenericMetricsCommandLineArgs();
            args.EndDate = new DateTime(2018, 9, 17, 11, 00, 00);
            args.Generate7Days = "true";

            var converters = builder.Build(args);
            Assert.Equal(2, converters.Count);
            var changesMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.CHANGES_METRIC_KEY + "_7d").First();
            var linesChangedMeasure = (MeasureConverter)converters.Where(c => ((MeasureConverter)c).Metric.MetricKey == MeasureConverterListBuilder.LINES_CHANGED_METRIC_KEY + "_7d").First();
            Assert.Equal(new DateTime(2018, 9, 9, 00, 00, 00), changesMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), changesMeasure.EndDate);
            Assert.Equal(new DateTime(2018, 9, 9, 00, 00, 00), linesChangedMeasure.StartDate);
            Assert.Equal(new DateTime(2018, 9, 17, 00, 00, 00), linesChangedMeasure.EndDate);
        }
    }
}
