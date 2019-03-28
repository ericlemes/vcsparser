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
    public class GivenANumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator
    {
        private NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator measureAggregator;

        public GivenANumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator()
        {
            this.measureAggregator = new NumberOfAuthorsWithMoreThanTenPercentChangesMeasureAggregator();
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
                        NumberOfChanges = 50
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 49
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 1
                    }
                }
            };

            Assert.Equal(2, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn));
        }

        [Fact]
        public void WhenGettingValueForNewMeasureDifferentFileShouldNotSumNumberOfChanges()
        {
            var dailyCodeChurn1 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 50
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 49
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 1
                    }
                }
            };

            var dailyCodeChurn2 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file2",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author4",
                        NumberOfChanges = 50
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author5",
                        NumberOfChanges = 49
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 1
                    }
                }
            };

            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn1);
            Assert.Equal(2, this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn2));
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
                        NumberOfChanges = 10
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 20
                    }
                }
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn);

            var measure = new Measure<int>();
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
                        NumberOfChanges = 67
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 3
                    }
                }
            };

            Assert.Equal(2, this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, measure));
        }

        [Fact]
        public void WhenGettingValueForprojectMeasureSameFileShouldReturnUniqueAuthors()
        {
            var dailyCodeChurn1 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 50
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 49
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 1
                    }
                }
            };
            var dailyCodeChurn2 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/18 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 51
                    }
                }
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn1);
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn2);

            Assert.Equal(3, this.measureAggregator.GetValueForProjectMeasure(dailyCodeChurn1));
        }

        [Fact]
        public void WhenGettingValueForprojectMeasureDiffrentFilesShouldReturnUniqueAuthors()
        {
            var dailyCodeChurn1 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/17 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author1",
                        NumberOfChanges = 50
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author2",
                        NumberOfChanges = 49
                    },
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 1
                    }
                }
            };
            var dailyCodeChurn2 = new DailyCodeChurn()
            {
                Timestamp = "2018/09/18 00:00:00",
                FileName = "file2",
                Authors = new List<DailyCodeChurnAuthor>()
                {
                    new DailyCodeChurnAuthor()
                    {
                        Author = "author3",
                        NumberOfChanges = 51
                    }
                }
            };
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn1);
            this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn2);

            Assert.Equal(3, this.measureAggregator.GetValueForProjectMeasure(dailyCodeChurn1));
        }
    }
}
