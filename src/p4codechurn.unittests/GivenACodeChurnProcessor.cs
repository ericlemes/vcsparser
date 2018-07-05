using Moq;
using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenACodeChurnProcessor
    {
        private CodeChurnProcessor processor;
        private Mock<IProcessWrapper> processWrapperMock;
        private Mock<IChangesParser> changesParserMock;
        private Mock<IDescribeParser> describeParserMock;
        private Mock<ICommandLineParser> commandLineParserMock;
        private Mock<ILogger> loggerMock;
        private Mock<IStopWatch> stopWatchMock;

        public GivenACodeChurnProcessor()
        {
            var changesMemoryStream = new MemoryStream();            

            this.processWrapperMock = new Mock<IProcessWrapper>();
            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);

            this.changesParserMock = new Mock<IChangesParser>();
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>());

            this.describeParserMock = new Mock<IDescribeParser>();
            this.commandLineParserMock = new Mock<ICommandLineParser>();
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("changes commandline")).Returns(new Tuple<string, string>("changes", "commandline"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 1")).Returns(new Tuple<string, string>("describe", "1"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 2")).Returns(new Tuple<string, string>("describe", "2"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 3")).Returns(new Tuple<string, string>("describe", "3"));
            this.loggerMock = new Mock<ILogger>();

            this.stopWatchMock = new Mock<IStopWatch>();

            this.processor = new CodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, loggerMock.Object, stopWatchMock.Object);

        }

        [Fact]
        public void WhenProcessingShouldInvokeChangesCommandLine()
        {
            this.processor.Process("changes commandline", "describe {0}");

            this.processWrapperMock.Verify(m => m.Invoke("changes", "commandline"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldParseChanges()
        {
            var ms = new MemoryStream();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(ms);
            this.changesParserMock.Setup(m => m.Parse(ms)).Returns(new List<int>());

            this.processor.Process("changes commandline", "describe {0}");

            this.changesParserMock.Verify(m => m.Parse(ms), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldInvokeDescribeParserForEachChange()
        {
            var changesMemoryStream = new MemoryStream();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>() { 1, 2 });            

            this.processor.Process("changes commandline", "describe {0}");
            this.processWrapperMock.Verify(m => m.Invoke("describe", "1"), Times.Once());
            this.processWrapperMock.Verify(m => m.Invoke("describe", "2"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldParseDescribes()
        {
            var changesMemoryStream = new MemoryStream();
            var describeMemoryStream1 = new MemoryStream();
            var describeMemoryStream2 = new MemoryStream();

            var changeset1 = new Changeset();
            var changeset2 = new Changeset();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(describeMemoryStream1);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(describeMemoryStream2);
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>() { 1, 2 });
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream2)).Returns(changeset2);

            this.processor.Process("changes commandline", "describe {0}");

            this.describeParserMock.Verify(m => m.Parse(describeMemoryStream1), Times.Once());
            this.describeParserMock.Verify(m => m.Parse(describeMemoryStream2), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldReturnExpectedResults()
        {
            var changesMemoryStream = new MemoryStream();
            var describeMemoryStream1 = new MemoryStream();
            var describeMemoryStream2 = new MemoryStream();
            var describeMemoryStream3 = new MemoryStream();

            var changeset1 = new Changeset()
            {
                Timestamp = new DateTime(2018, 07, 05),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = "File1.cs",
                        Added = 1,
                        Deleted = 2,
                        ChangedBefore = 10,
                        ChangedAfter = 5
                    },
                    new FileChanges()
                    {
                        FileName = "File2.cs",
                        Added = 1,
                        Deleted = 2,
                        ChangedBefore = 10,
                        ChangedAfter = 5
                    }
                }
            };

            var changeset2 = new Changeset()
            {
                Timestamp = new DateTime(2018, 07, 05, 10, 0, 0),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = "File1.cs",
                        Added = 1,
                        Deleted = 2,
                        ChangedBefore = 10,
                        ChangedAfter = 5
                    }
                }
            };

            var changeset3 = new Changeset()
            {
                Timestamp = new DateTime(2018, 07, 06, 10, 0, 0),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = "File1.cs",
                        Added = 1,
                        Deleted = 2,
                        ChangedBefore = 10,
                        ChangedAfter = 5
                    }
                }
            };

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(describeMemoryStream1);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(describeMemoryStream2);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "3")).Returns(describeMemoryStream3);
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>() { 1, 2, 3 });
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream2)).Returns(changeset2);
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream3)).Returns(changeset3);

            var result = this.processor.Process("changes commandline", "describe {0}");

            Assert.Equal(3, result.Count);
            Assert.Equal(new DateTime(2018, 07, 05), result[0].Timestamp);
            Assert.Equal("File1.cs", result[0].FileName);
            Assert.Equal(2, result[0].Added);
            Assert.Equal(4, result[0].Deleted);
            Assert.Equal(20, result[0].ChangesBefore);
            Assert.Equal(10, result[0].ChangesAfter);

            Assert.Equal(new DateTime(2018, 07, 05), result[1].Timestamp);
            Assert.Equal("File2.cs", result[1].FileName);
            Assert.Equal(1, result[1].Added);
            Assert.Equal(2, result[1].Deleted);
            Assert.Equal(10, result[1].ChangesBefore);
            Assert.Equal(5, result[1].ChangesAfter);

            Assert.Equal(new DateTime(2018, 07, 06), result[2].Timestamp);
            Assert.Equal("File1.cs", result[2].FileName);
            Assert.Equal(1, result[2].Added);
            Assert.Equal(2, result[2].Deleted);
            Assert.Equal(10, result[2].ChangesBefore);
            Assert.Equal(5, result[2].ChangesAfter);
        }

        [Fact]
        public void WhenProcessingShouldLogProgressEvery60Seconds()
        {
            var changesMemoryStream = new MemoryStream();
            var describeMemoryStream1 = new MemoryStream();
            var describeMemoryStream2 = new MemoryStream();

            var changeset1 = new Changeset();
            var changeset2 = new Changeset();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(describeMemoryStream1);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(describeMemoryStream2);
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>() { 1, 2 });
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream2)).Returns(changeset2);

            this.stopWatchMock.SetupSequence(m => m.TotalSecondsElapsed()).Returns(10).Returns(70);

            this.processor.Process("changes commandline", "describe {0}");

            this.loggerMock.Verify(m => m.LogToConsole("Processed 1/2 changesets"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldNotReturnResultsWithNoChurn()
        {
            var changesMemoryStream = new MemoryStream();
            var describeMemoryStream1 = new MemoryStream();

            var changeset1 = new Changeset()
            {
                Timestamp = new DateTime(2018, 07, 05),
                FileChanges = new List<FileChanges>()
                {
                    new FileChanges()
                    {
                        FileName = "File1.cs",
                        Added = 0,
                        Deleted = 0,
                        ChangedBefore = 0,
                        ChangedAfter = 0
                    }
                }
            };

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(changesMemoryStream);
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(describeMemoryStream1);
            this.changesParserMock.Setup(m => m.Parse(changesMemoryStream)).Returns(new List<int>() { 1 });
            this.describeParserMock.Setup(m => m.Parse(describeMemoryStream1)).Returns(changeset1);

            var result = this.processor.Process("changes commandline", "describe {0}");

            Assert.Equal(0, result.Count);
        }


    }
}
