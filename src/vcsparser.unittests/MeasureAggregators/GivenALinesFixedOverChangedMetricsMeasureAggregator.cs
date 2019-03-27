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
        public void WhenGettingValueForNewMeasureShouldReturnPercentage()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 4,
                AddedWithFixes = 5,
                DeletedWithFixes = 2
            };

            Assert.Equal(50, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForExistingMeasureShouldReturnPercentage()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 4,
                AddedWithFixes = 5,
                DeletedWithFixes = 2
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn);

            var dailyCodeChurn2 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 20,
                AddedWithFixes = 5,
            };

            var measure = new Measure<double>();
            Assert.Equal(35.294117647058823, this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn2, measure));
        }

        [Fact]
        public void WhenGettingValueForProjectMeasureShouldReturnPercentage()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 10,
                Deleted = 4,
                AddedWithFixes = 5,
                DeletedWithFixes = 2
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn);

            Assert.Equal(50, this.measureAggregator.GetValueForProjectMeasure(dailyCodeChurn));
        }
    }
}
