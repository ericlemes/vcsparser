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
    public class GivenAMeasureConverterRaw
    {
        private MeasureConverterRaw<string> measureConverter;
        private Metric metric;
        private Mock<IMeasureAggregator<string>> mockMeasureAggregator;

        public GivenAMeasureConverterRaw()
        {
            metric = new Metric();
            metric.MetricKey = "key";

            this.mockMeasureAggregator = new Mock<IMeasureAggregator<string>>();
            this.mockMeasureAggregator.Setup(m => m.HasValue(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.FileName.Length > 0);
            this.mockMeasureAggregator.Setup(m => m.GetValueForNewMeasure(It.IsAny<DailyCodeChurn>())).
                Returns((DailyCodeChurn d) => d.FileName);
            this.mockMeasureAggregator.Setup(m => m.GetValueForExistingMeasure(It.IsAny<DailyCodeChurn>(), It.IsAny<Measure<string>>())).
                Returns((DailyCodeChurn d, Measure<string> existingMeasure) => d.FileName + "," + existingMeasure.Value);

            this.measureConverter = new MeasureConverterRaw<string>(metric, mockMeasureAggregator.Object, "//prefix/");
        }

        [Fact]
        public void WhenConvertingShouldAdd()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 12:00:00",
                FileName = "file1"
            };
            var measures = new SonarMeasuresJson();
            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal("file1", (Assert.Single(measures.MeasuresRaw) as Measure<string>).Value);
        }

        [Fact]
        public void WhenConvertingAndAlreadyHaveExistingMeasureShouldUpdate()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1"
            };
            var measures = new SonarMeasuresJson();
            measures.AddRawMeasure(new Measure<string>()
            {
                MetricKey = "key",
                File = "file1",
                Value = "file1"
            });

            this.measureConverter.ProcessFileMeasure(dailyCodeChurn, measures);

            Assert.Equal("file1,file1",
                measures.MeasuresRaw.Where(m => m.MetricKey == "key" && m.File == dailyCodeChurn.FileName).Select(m => m as Measure<string>).Single().Value);
        }

        [Fact]
        public void WhenConvertingWithProjectMeausreShouldNotThrow()
        {
            var measures = new SonarMeasuresJson();

            try
            {
                this.measureConverter.ProcessProjectMeasure(measures);
                Assert.True(true);
            }
            catch (Exception)
            {
                Assert.True(false);
            }
        }
    }
}
