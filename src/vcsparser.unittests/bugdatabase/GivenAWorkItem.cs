using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenAWorkItem
    {
        [Fact]
        public void WhenCompareDateAfterReturn1()
        {
            WorkItem item1 = new WorkItem
            {
                ClosedDate = "2019-04-16 00:00:00",
            };

            WorkItem item2 = new WorkItem
            {
                ClosedDate = "2019-04-17 00:00:00",
            };

            int compare = item2.CompareTo(item1);

            Assert.Equal(1, compare);
        }

        [Fact]
        public void WhenCompareSameDateCompareIdThenStringCompare()
        {
            WorkItem item1 = new WorkItem
            {
                ClosedDate = "2019-04-16 00:00:00",
                WorkItemId = "abc"
            };

            WorkItem item2 = new WorkItem
            {
                ClosedDate = "2019-04-16 00:00:00",
                WorkItemId = "def"
            };

            int compare = item2.CompareTo(item1);

            Assert.Equal(1, compare);
        }

        [Fact]
        public void WhenCompareSameItemThenReturnZero()
        {
            WorkItem item = new WorkItem
            {
                ClosedDate = "2019-04-16 00:00:00",
                WorkItemId = "abc"
            };

            int compare = item.CompareTo(item);

            Assert.Equal(0, compare);
        }
    }
}
