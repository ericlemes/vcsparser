using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAMeasureConverter
    {
        private MeasureConverter measureConverter;
        private Metric metric;

        public GivenAMeasureConverter()
        {
            metric = new Metric();
            metric.MetricKey = "key";
            this.measureConverter = new MeasureConverter(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, MeasureConverterType.LinesChanged, "//prefix/");
        }

        [Fact]
        public void WhenConvertingWithinRangeShouldAppendMetrics()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = new DateTime(2018, 9, 17),
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
                Timestamp = new DateTime(2018, 9, 17),
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
                Timestamp = new DateTime(2018, 9, 17),
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
        public void WhenConvertingWithinRangeShouldAppendMeasures()
        {           
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = new DateTime(2018, 9, 17),
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.Process(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged, 
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Single().Value);            
        }

        [Fact]
        public void WhenConvertingWithinRangeAndNumChangesShouldAppendNumChanges()
        {
            this.measureConverter = new MeasureConverter(new DateTime(2018, 9, 17), new DateTime(2018, 9, 18), metric, MeasureConverterType.NumberOfChanges, "//prefix/");
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = new DateTime(2018, 9, 17),
                FileName = "file1",
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 1
            };
            var measures = new SonarMeasuresJson();

            this.measureConverter.Process(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.NumberOfChanges,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Single().Value);
        }

        [Fact]
        public void WhenConvertingAndThereIsExistingDataSouldKeepExistingData()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = new DateTime(2018, 9, 17),
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            measures.Measures.Add(new Measure()
            {
                MetricKey = "key2",
                File = "file1",
                Value = 5
            });

            this.measureConverter.Process(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Single().Value);
            Assert.Equal(2, measures.Measures.Count());
        }

        [Fact]
        public void WhenConvertingAndOutOfRangeShouldDoNothing()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = new DateTime(2018, 9, 18, 12, 00, 00),
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
                Timestamp = new DateTime(2018, 9, 17, 12, 00, 00),
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
                Timestamp = new DateTime(2018, 9, 17),
                FileName = "file1",
                Added = 10,
                Deleted = 10
            };
            var measures = new SonarMeasuresJson();
            measures.Measures.Add(new Measure()
            {
                MetricKey = "key",
                File = "file1",
                Value = 5
            });

            this.measureConverter.Process(dailyCodeChurn, measures);

            Assert.Equal(dailyCodeChurn.TotalLinesChanged + 5,
                measures.Measures.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Single().Value);            
        }
    }
}
