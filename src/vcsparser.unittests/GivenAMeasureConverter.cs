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

            this.measureConverter.Process(dailyCodeChurn, measures);

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

            this.measureConverter.Process(dailyCodeChurn, measures);
            this.measureConverter.Process(dailyCodeChurn, measures);

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

            this.measureConverter.Process(dailyCodeChurn, measures);

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

            this.measureConverter.Process(dailyCodeChurn, measures);

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

            this.measureConverter.Process(dailyCodeChurn, measures);

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
            measures.Measures.Add(new Measure<object>()
            {
                MetricKey = "key2",
                File = "file1",
                Value = 5
            });

            this.measureConverter.Process(dailyCodeChurn, measures);

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
            this.measureConverter.Process(dailyCodeChurn, measures);

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
            this.measureConverter.Process(dailyCodeChurn, measures);

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
            this.measureConverter.Process(dailyCodeChurn, measures);

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
            measures.AddMeasure(new Measure<int>()
            {
                MetricKey = "key",
                File = "file1",
                Value = 5
            });

            this.measureConverter.Process(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged + 5,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Select(m => m as Measure<int>).Single().Value);            
        }
    }
}
