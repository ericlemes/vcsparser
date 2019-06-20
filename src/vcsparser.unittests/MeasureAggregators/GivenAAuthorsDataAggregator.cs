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
    public class GivenAAuthorsDataAggregator
    {
        private AuthorsDataAggregator measureAggregator;

        public GivenAAuthorsDataAggregator()
        {
            measureAggregator = new AuthorsDataAggregator();
        }

        [Fact]
        public void WhenCallingHasValueAndNoAuthorsShouldReturnFalse()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2019/06/20 00:00:00",
                FileName = "file1"
            };

            Assert.False(this.measureAggregator.HasValue(dailyCodeChurn));
        }

        [Fact]
        public void WhenCallingHasValueAndNoAuthorsShouldReturnTrue()
        {
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2019/06/20 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor>
                {
                    new DailyCodeChurnAuthor
                    {
                        Author = "An Author Name",
                        NumberOfChanges = 2
                    }
                }
            };

            Assert.True(this.measureAggregator.HasValue(dailyCodeChurn));
        }

        [Fact]
        public void WhenCallingGetValueForNewMeasureShouldReturnAuthorDataList()
        {
            var author = new DailyCodeChurnAuthor
            {
                Author = "An Author Name",
                NumberOfChanges = 2
            };
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2019/06/20 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor> { author }
            };

            var list = this.measureAggregator.GetValueForNewMeasure(dailyCodeChurn);
            var authorData = Assert.Single(list);
            Assert.Equal("2019/06/20", authorData.Date);
            Assert.Equal(author, Assert.Single(authorData.Authors));
        }

        [Fact]
        public void WhenCallingGetValueForExistingMeasureShouldAddAuthorToMeasure()
        {
            var author1 = new DailyCodeChurnAuthor
            {
                Author = "Author 1",
                NumberOfChanges = 2
            };
            var author2 = new DailyCodeChurnAuthor
            {
                Author = "Author 2",
                NumberOfChanges = 3
            };
            var dailyCodeChurn = new DailyCodeChurn()
            {
                Timestamp = "2019/06/20 00:00:00",
                FileName = "file1",
                Authors = new List<DailyCodeChurnAuthor> { author2 }
            };

            var measure = new Measure<List<AuthorsData>>();
            measure.Value = new List<AuthorsData> { new AuthorsData { Authors = new List<DailyCodeChurnAuthor> { author1 } } };

            var list = this.measureAggregator.GetValueForExistingMeasure(dailyCodeChurn, measure);

            Assert.Equal(2, list.Count);
            Assert.Equal("Author 1", Assert.Single(list.First().Authors).Author);
            Assert.Equal("Author 2", Assert.Single(list.Last().Authors).Author);
        }
    }
}
