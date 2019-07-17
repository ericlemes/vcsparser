using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;

namespace vcsparser.unittests
{
    public class GivenAMeasureConverter
    {
        private MeasureConverter<int> measureConverter;
        private Metric metric;
        private Mock<IMeasureAggregator<int>> mockMeasureAggregator;

        private Mock<IMeasureAggregatorProject<int>> mockMeasureAggregatorProject;

        public GivenAMeasureConverter()
        {
            metric = new Metric();
            metric.MetricKey = "key";

            this.mockMeasureAggregator = new Mock<IMeasureAggregator<int>>();
            this.mockMeasureAggregator.Setup(m => m.HasValue(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.TotalLinesChanged > 0);
            this.mockMeasureAggregator.Setup(m => m.GetValueForNewMeasure(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.TotalLinesChanged);
            this.mockMeasureAggregator.Setup(m => m.GetValueForExistingMeasure(It.IsAny<DailyCodeChurn>(), It.IsAny<Measure<int>>())).
                Returns((DailyCodeChurn d, Measure<int> existingMeasure) => d.TotalLinesChanged + existingMeasure.Value);

            this.mockMeasureAggregatorProject = new Mock<IMeasureAggregatorProject<int>>();
            this.mockMeasureAggregatorProject.Setup(m => m.HasValue(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.TotalLinesChanged > 0);
            this.mockMeasureAggregatorProject.Setup(m => m.GetValueForNewMeasure(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.TotalLinesChanged);
            this.mockMeasureAggregatorProject.Setup(m => m.GetValueForExistingMeasure(It.IsAny<DailyCodeChurn>(), It.IsAny<Measure<int>>())).
                Returns((DailyCodeChurn d, Measure<int> existingMeasure) => d.TotalLinesChanged + existingMeasure.Value);
            this.mockMeasureAggregatorProject.Setup(m => m.GetValueForProjectMeasure()).
               Returns(0);

            this.measureConverter = new MeasureConverter<int>(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, mockMeasureAggregator.Object, "//prefix/");
        }

        [Fact]
        public void WhenConvertingWithinRangeShouldAppendMetrics()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.NotEmpty(measures.Metrics.Where(m => m.MetricKey == "key"));
        }

        [Fact]
        public void WhenConvertingWithinRangeSameMetricTwiceShouldAppendMetricsOnce()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);
            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Single(measures.Metrics.Where(m => m.MetricKey == "key"));
        }

        [Fact]
        public void WhenConvertingWithinRangeShouldRemoveFilePrefix()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "//prefix/file",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal("file",
                measures.Measures.Where(m => m.MetricKey == "key").Single().File);
        }

        [Fact]
        public void WhenConvertingWithinRangeAndDifferentCaseShouldRemoveFilePrefix()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "//PREfix/file",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal("file",
                measures.Measures.Where(m => m.MetricKey == "key").Single().File);
        }

        [Fact]
        public void WhenConvertingWithinRangeAndNoFilePrefixShouldConvert()
        {
            this.measureConverter = new MeasureConverter<int>(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, mockMeasureAggregator.Object, null);
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "//prefix/file",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal("//prefix/file",
                measures.Measures.Where(m => m.MetricKey == "key").Single().File);
        }

        [Fact]
        public void WhenConvertingWithinRangeShouldAppendMeasures()
        {           
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged, 
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Select(m => m as Measure<int>).Single().Value);            
        }

        [Fact]
        public void WhenConvertingAndThereIsExistingDataSouldKeepExistingData()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            measures.Measures.Add(new Measure<int>()
            {
                MetricKey = "key2",
                File = "file1",
                Value = 5
            });

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Select(m => m as Measure<int>).Single().Value);
            Assert.Equal(2, measures.Measures.Count());
        }

        [Fact]
        public void WhenConvertingAndAfterRangeShouldDoNothing()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/18 12:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Empty(measures.Measures);
        }

        [Fact]
        public void WhenConvertingAndBeforeRangeShouldDoNothing()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/16 12:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Empty(measures.Measures);
        }

        [Fact]
        public void WhenConvertingAndInRangeAndValueZeroShouldDoNothing()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 12:00:00",
                FileName = "file1",
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 0
            };
            var measures = new SonarMeasuresJson();
            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Empty(measures.Measures);
        }

        [Fact]
        public void WhenConvertingAndAlreadyHaveExistingMeasureShouldUpdate()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            measures.AddFileMeasure(new Measure<int>()
            {
                MetricKey = "key",
                File = "file1",
                Value = 5
            });

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged + 5,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Select(m => m as Measure<int>).Single().Value);            
        }

        [Fact]
        public void WhenConvertingNoProjectMeausreShouldAppendProjectMeasure()
        {
            this.measureConverter = new MeasureConverter<int>(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, mockMeasureAggregatorProject.Object, "//prefix/");

            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessProjectMeasure(measures);

            Assert.NotEmpty(measures.MeasuresProject.Where(m => m.MetricKey == "key"));
        }

        [Fact]
        public void WhenConvertingANonProjectAggregatorWithProjectMeausreShouldDoNothing()
        {
            this.measureConverter = new MeasureConverter<int>(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, mockMeasureAggregator.Object, "//prefix/");

            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessProjectMeasure(measures);

            Assert.Empty(measures.MeasuresProject.Where(m => m.MetricKey == "key"));
        }

        [Fact]
        public void WhenConvertingWithProjectMeausreShouldAppendProjectMeasure()
        {
            this.measureConverter = new MeasureConverter<int>(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, mockMeasureAggregatorProject.Object, "//prefix/");

            var dailyCodeChurn1 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var dailyCodeChurn2 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file2",
                Added = 5,
                Deleted = 5
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.ProcessProjectMeasure(measures);
            this.measureConverter.ProcessProjectMeasure(measures);

            Assert.Single(measures.MeasuresProject.Where(m => m.MetricKey == "key"));
        }
    }
}
