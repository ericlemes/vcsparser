using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.MeasureAggregators;
using Xunit;

namespace vcsparser.unittests.MeasureAggregators
{
    public class GivenALinesFixedOverChangedMetricsMeasureAggregator
    {
        private LinesFixedOverChangedMetricsMeasureAggregator measureAggregator;

        public GivenALinesFixedOverChangedMetricsMeasureAggregator()
        {
            this.measureAggregator = new LinesFixedOverChangedMetricsMeasureAggregator();
        }

        [Fact]
        public void WhenCallingHasValueAndNoValueShouldReturnFalse()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 0,
                Deleted = 0
            };

            Assert.False(this.measureAggregator.HasValue(dailyCodeChurn));
        }

        [Fact]
        public void WhenCallingAHasValueAndHasValueShouldReturnTrue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 6,
                Deleted = 1
            };

            Assert.True(this.measureAggregator.HasValue(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForNewMeasureShouldReturnNumberOfChanges()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 6,
                Deleted = 1,
                AddedWithFixes = 4,
                DeletedWithFixes = 2
            };

            Assert.Equal(85.714285714285714285714285714286, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }
    }
}
