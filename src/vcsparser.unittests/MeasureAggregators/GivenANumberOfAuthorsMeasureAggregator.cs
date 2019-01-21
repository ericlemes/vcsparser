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
    public class GivenANumberOfAuthorsMeasureAggregator
    {        
        private NumberOfAuthorsMeasureAggregator measureAggregator;

        public GivenANumberOfAuthorsMeasureAggregator()
        {
            this.measureAggregator = new NumberOfAuthorsMeasureAggregator();
        }

        [Fact]
        public void WhenCallingHasValueShouldReturnTrue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1"                
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
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 1
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 2
                    }
                }                
            };

            Assert.Equal(2, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForExistingMeasureShouldSumWithExistingValue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 1
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 2
                    }
                }
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn);

            var measure = new Measure();
            measure.Value = 3;
            dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 5
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 3
                    }
                }
            };

            Assert.Equal(3, this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, measure));
        }

    }
}
