using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAAuthorsData
    {
        [Fact]
        public void WhenDateThenReturnFormatTimeStamp()
        {
            var data = new AuthorsData {
                Timestamp = new DateTime(2019,06,20)
            };

            var date = data.Date;

            Assert.Equal("2019/06/20", date);
        }
    }
}
