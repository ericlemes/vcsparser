using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAJsonOutputData
    {
        [Fact]
        public void WhenGettingCurrentVersionTDailiyCodeChurnThenReturnExpectedVersion()
        {
            var version = JsonOutputData<DailyCodeChurn>.CurrentVersion;

            Assert.Equal(JsonOutputData<DailyCodeChurn>.DailyCodeChurn, version);
        }

        [Fact]
        public void WhenGettingCurrentVersionTWorkItemThenReturnExpectedVersion()
        {
            var version = JsonOutputData<WorkItem>.CurrentVersion;

            Assert.Equal(JsonOutputData<WorkItem>.BugDatabase, version);
        }

        [Fact]
        public void WhenGettingCurrentVersionTUnimplementedThenReturnExpectedVersion()
        {
            var version = JsonOutputData<SomeUnimplementedClass>.CurrentVersion;

            Assert.Equal(0, version);
        }
    }
}
