using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.p4;
using Xunit;

namespace vcsparser.unittests.p4
{
    public class GivenAPerforceWorkItemConverter
    {
        private Mock<ICommandLineParser> commandLineParserMock;
        private Mock<IProcessWrapper> processWrapperMock;
        private Mock<IDescribeParser> describeParserMock;

        private PerforceWorkItemConverter workItemConverter;

        public GivenAPerforceWorkItemConverter()
        {
            this.commandLineParserMock = new Mock<ICommandLineParser>();
            this.commandLineParserMock.Setup((c) => c.ParseCommandLine(It.IsAny<string>())).Returns(new Tuple<string, string>("Some Item 1", "Some Item 2"));

            this.processWrapperMock = new Mock<IProcessWrapper>();
            this.processWrapperMock.Setup((p) => p.Invoke(It.IsAny<string>(), It.IsAny<string>())).Returns((Stream)null);

            this.describeParserMock = new Mock<IDescribeParser>();
            this.describeParserMock.Setup((d) => d.Parse(It.IsAny<Stream>())).Returns(new PerforceChangeset());

            this.workItemConverter = new PerforceWorkItemConverter(this.commandLineParserMock.Object, this.processWrapperMock.Object, this.describeParserMock.Object);
        }

        [Fact]
        public void WhenConvertEmptyList_ThenDontConvert()
        {
            IEnumerable<WorkItem> items = new List<WorkItem>();

            var convertedList = this.workItemConverter.Convert(items).ToArray();

            Assert.Empty(convertedList);
        }

        [Fact]
        public void WhenConvertingInvalidChangesetId_ThenReturnNull()
        {
            IEnumerable<WorkItem> items = new List<WorkItem>()
            {
                new WorkItem {
                    ChangesetId = "SomeNotANumber"
                }
            };

            var convertedList = this.workItemConverter.Convert(items).ToArray();

            Assert.Empty(convertedList);
        }

        [Fact]
        public void WhenConvertingValidItem_ThenReturnConvertedItem()
        {
            IEnumerable<WorkItem> items = new List<WorkItem>()
            {
                new WorkItem { ChangesetId = "1" }
            };

            var convertedList = this.workItemConverter.Convert(items).ToArray();

            Assert.Single(convertedList);
        }

        [Fact]
        public void WhenConvertingInvalidItem_ThenReturnConvertedList()
        {
            this.describeParserMock.SetupSequence((g) => g.Parse(It.IsAny<Stream>()))
                .Returns(new PerforceChangeset())
                .Returns((PerforceChangeset)null);

            IEnumerable<WorkItem> items = new List<WorkItem>()
            {
                new WorkItem { ChangesetId = "1" },
                new WorkItem { ChangesetId = "2" }
            };

            var convertedList = this.workItemConverter.Convert(items).ToArray();

            this.describeParserMock.Verify((g) => g.Parse(It.IsAny<Stream>()), Times.Exactly(2));
            Assert.Single(convertedList);
        }
    }
}
