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
                ClosedDate = new DateTime(0),
            };

            WorkItem item2 = new WorkItem
            {
                ClosedDate = new DateTime(1),
            };

            int compare = item2.CompareTo(item1);

            Assert.Equal(1, compare);
        }

        [Fact]
        public void WhenCompareSameDateCompareIdThenStringCompare()
        {
            WorkItem item1 = new WorkItem
            {
                ClosedDate = new DateTime(2019, 04, 16),
                WorkItemId = "abc"
            };

            WorkItem item2 = new WorkItem
            {
                ClosedDate = new DateTime(2019, 04, 16),
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
                ClosedDate = new DateTime(2019, 04, 16),
                WorkItemId = "abc"
            };

            int compare = item.CompareTo(item);

            Assert.Equal(0, compare);
        }
    }
}
