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
    public class GivenALinesChangedInFixesMeasureAggregator
    {
        private LinesChangedWithFixesMeasureAggregator measureAggregator;

        public GivenALinesChangedInFixesMeasureAggregator()
        {
            this.measureAggregator = new LinesChangedWithFixesMeasureAggregator();
        }

        [Fact]
        public void WhenCallingHasValueAndNoValueShouldReturnFalse()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                AddedWithFixes = 0,
                DeletedWithFixes = 0                
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
                AddedWithFixes = 1,
                DeletedWithFixes = 1                
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
                AddedWithFixes = 5,
                DeletedWithFixes = 1                
            };

            Assert.Equal(6, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForExistingMeasureShouldSumWithExistingValue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                AddedWithFixes = 5,
                DeletedWithFixes = 1                
            };
            var measure = new Measure<int>();
            measure.Value = 3;
            Assert.Equal(9, this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, measure));
        }

    }
}
