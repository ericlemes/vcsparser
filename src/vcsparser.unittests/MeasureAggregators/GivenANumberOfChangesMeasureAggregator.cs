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
    public class GivenANumberOfChangesMeasureAggregator
    {
        private NumberOfChangesMeasureAggregator measureAggregator;

        public GivenANumberOfChangesMeasureAggregator()
        {
            this.measureAggregator = new NumberOfChangesMeasureAggregator();
        }

        [Fact]
        public void WhenCallingHasValueAndNoValueShouldReturnFalse()
        {            
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 0
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
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 1
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
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 5
            };

            Assert.Equal(5, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForExistingMeasureShouldSumWithExistingValue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Added = 0,
                Deleted = 0,
                NumberOfChanges = 5
            };
            var measure = new Measure<int>();
            measure.Value = 3;
            Assert.Equal(8, this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, measure));
        }


    }
}
