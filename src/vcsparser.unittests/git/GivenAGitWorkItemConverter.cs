using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using vcsparser.core;
using vcsparser.core.git;
using System.IO;
using vcsparser.core.bugdatabase;

namespace vcsparser.unittests.git
{
    public class GivenAGitWorkItemConverter
    {
        private Mock<ICommandLineParser> commandLineParserMock;
        private Mock<IProcessWrapper> processWrapperMock;
        private Mock<IGitLogParser> gitLogParserMock;

        private GitWorkItemConverter workItemConverter;

        public GivenAGitWorkItemConverter()
        {
            this.commandLineParserMock = new Mock<ICommandLineParser>();
            this.commandLineParserMock.Setup((c) => c.ParseCommandLine(It.IsAny<string>())).Returns(new Tuple<string, string>("Some Item 1", "Some Item 2"));

            this.processWrapperMock = new Mock<IProcessWrapper>();
            this.processWrapperMock.Setup((p) => p.Invoke(It.IsAny<string>(), It.IsAny<string>())).Returns((Stream) null);

            this.gitLogParserMock = new Mock<IGitLogParser>();
            this.gitLogParserMock.Setup((g) => g.Parse(It.IsAny<Stream>())).Returns(new List<GitCommit>() { new GitCommit() });

            this.workItemConverter = new GitWorkItemConverter(this.commandLineParserMock.Object, this.processWrapperMock.Object, this.gitLogParserMock.Object);
        }

        //[Fact]
        //public void WhenConvertEmptyList_ThenDontConvert()
        //{
        //    IEnumerable<WorkItem> items = new List<WorkItem>();

        //    var convertedList = this.workItemConverter.Convert(items).ToArray();

        //    Assert.Empty(convertedList);
        //}

        //[Fact]
        //public void WhenConvertingValidItem_ThenReturnConvertedItem()
        //{
        //    IEnumerable<WorkItem> items = new List<WorkItem>()
        //    {
        //        new WorkItem()
        //    };

        //    var convertedList = this.workItemConverter.Convert(items).ToArray();

        //    Assert.Single(convertedList);
        //}

        //[Fact]
        //public void WhenConvertingInvalidItem_ThenReturnConvertedList()
        //{
        //    this.gitLogParserMock.SetupSequence((g) => g.Parse(It.IsAny<Stream>()))
        //        .Returns(new List<GitCommit>() { new GitCommit() })
        //        .Returns(new List<GitCommit>() { });

        //    IEnumerable<WorkItem> items = new List<WorkItem>()
        //    {
        //        new WorkItem(),
        //        new WorkItem()
        //    };

        //    var convertedList = this.workItemConverter.Convert(items).ToArray();

        //    this.gitLogParserMock.Verify((g) => g.Parse(It.IsAny<Stream>()), Times.Exactly(2));
        //    Assert.Single(convertedList);
        //}
    }
}
