using Moq;
using vcsparser.core;
using vcsparser.core.p4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using vcsparser.core.bugdatabase;

namespace vcsparser.unittests
{
    public class GivenAPerforceCodeChurnProcessor
    {
        private PerforceCodeChurnProcessor processor;
        private P4ExtractCommandLineArgs commandLineArgs;
        private Mock<IProcessWrapper> processWrapperMock;
        private Mock<IChangesParser> changesParserMock;
        private Mock<IDescribeParser> describeParserMock;
        private Mock<ICommandLineParser> commandLineParserMock;
        private Mock<ILogger> loggerMock;
        private Mock<IStopWatch> stopWatchMock;
        private Mock<IOutputProcessor> outputProcessorMock;
        private Mock<IBugDatabaseProcessor> bugDatabseMock;
        private Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> output;

        public GivenAPerforceCodeChurnProcessor()
        {
            var invokeLines = new List<string>();
            this.processWrapperMock = new Mock<IProcessWrapper>();
            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, invokeLines));

            this.changesParserMock = new Mock<IChangesParser>();
            this.changesParserMock.Setup(m => m.Parse(invokeLines)).Returns(new List<int>());

            this.describeParserMock = new Mock<IDescribeParser>();
            this.commandLineParserMock = new Mock<ICommandLineParser>();
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("changes commandline")).Returns(new Tuple<string, string>("changes", "commandline"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 1")).Returns(new Tuple<string, string>("describe", "1"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 2")).Returns(new Tuple<string, string>("describe", "2"));
            this.commandLineParserMock.Setup(m => m.ParseCommandLine("describe 3")).Returns(new Tuple<string, string>("describe", "3"));
            this.loggerMock = new Mock<ILogger>();

            this.stopWatchMock = new Mock<IStopWatch>();

            this.outputProcessorMock = new Mock<IOutputProcessor>();
            this.outputProcessorMock.Setup(m => m.ProcessOutput(OutputType.SingleFile, It.IsAny<string>(), It.IsAny<Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>())).Callback<OutputType, string, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>(
                (outputType, file, output) =>
                {
                    this.output = output;
                }
            );

            this.bugDatabseMock = new Mock<IBugDatabaseProcessor>();

            this.commandLineArgs = new P4ExtractCommandLineArgs
            {
                OutputType = OutputType.SingleFile,
                OutputFile = "filename",
                P4ChangesCommandLine = "changes commandline",
                P4DescribeCommandLine = "describe {0}"
            };
            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, bugDatabseMock.Object, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

        }

        [Fact]
        public void WhenProcessingShouldInvokeChangesCommandLine()
        {
            this.processor.Extract();

            this.processWrapperMock.Verify(m => m.Invoke("changes", "commandline"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldParseChanges()
        {
            var invokeLines = new List<string>();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, invokeLines));
            this.changesParserMock.Setup(m => m.Parse(invokeLines)).Returns(new List<int>());

            this.processor.Extract();

            this.changesParserMock.Verify(m => m.Parse(invokeLines), Times.Once());
        }

        [Fact]
        public void WhenProcessingAndChangesNonZeroShouldReturnExitCode()
        {
            var invokeLines = new List<string>();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(1, invokeLines));

            var exitCode = this.processor.Extract();

            this.processWrapperMock.Verify(m => m.Invoke(It.IsAny<string>(), It.IsAny<string>()), Times.Once());
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void WhenProcessingShouldInvokeDescribeParserForEachChange()
        {
            var invokeLines = new List<string>();
            var describeLines1 = new List<string>();
            var describeLines2 = new List<string>();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, invokeLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(0, describeLines1));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(new Tuple<int, List<string>>(0, describeLines2));
            this.changesParserMock.Setup(m => m.Parse(invokeLines)).Returns(new List<int>() { 1, 2 });

            this.processor.Extract();
            this.processWrapperMock.Verify(m => m.Invoke("describe", "1"), Times.Once());
            this.processWrapperMock.Verify(m => m.Invoke("describe", "2"), Times.Once());
        }

        [Fact]
        public void WhenProcessingAndDescribeNonZeroShouldExitCode()
        {
            var invokeLines = new List<string>();
            var describeLines = new List<string>();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, invokeLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(1, describeLines));
            this.changesParserMock.Setup(m => m.Parse(invokeLines)).Returns(new List<int>() { 1 });

            var exitCode = this.processor.Extract();

            this.processWrapperMock.Verify(m => m.Invoke("describe", "1"), Times.Once());
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void WhenProcessingShouldParseDescribes()
        {
            var changesLines = new List<string>();
            var describeLines1 = new List<string>() { "describe 1" };
            var describeLines2 = new List<string>() { "describe 2" };

            var changeset1 = new PerforceChangeset();
            var changeset2 = new PerforceChangeset();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, changesLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(0, describeLines1));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(new Tuple<int, List<string>>(0, describeLines2));
            this.changesParserMock.Setup(m => m.Parse(changesLines)).Returns(new List<int>() { 1, 2 });
            this.describeParserMock.Setup(m => m.Parse(describeLines1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeLines2)).Returns(changeset2);

            this.processor.Extract();

            this.describeParserMock.Verify(m => m.Parse(describeLines1), Times.Once());
            this.describeParserMock.Verify(m => m.Parse(describeLines2), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldSaveOutputWithExpectedResults()
        {
            var changesLines = new List<string>();
            var describeLines1 = new List<string>() { "describe 1" };
            var describeLines2 = new List<string>() { "describe 2" };
            var describeLines3 = new List<string>() { "describe 3" };

            var changeset1 = new PerforceChangeset()
            {
                AuthorName = "author1",
                ChangesetTimestamp = new DateTime(2018, 07, 05),
                ChangesetFileChanges = new List<FileChanges>()
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

            var changeset2 = new PerforceChangeset()
            {
                AuthorName = "author1",
                ChangesetTimestamp = new DateTime(2018, 07, 05, 10, 0, 0),
                ChangesetFileChanges = new List<FileChanges>()
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

            var changeset3 = new PerforceChangeset()
            {
                AuthorName = "author1",
                ChangesetTimestamp = new DateTime(2018, 07, 06, 10, 0, 0),
                ChangesetFileChanges = new List<FileChanges>()
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

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, changesLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(0, describeLines1));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(new Tuple<int, List<string>>(0, describeLines2));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "3")).Returns(new Tuple<int, List<string>>(0, describeLines3));
            this.changesParserMock.Setup(m => m.Parse(changesLines)).Returns(new List<int>() { 1, 2, 3 });
            this.describeParserMock.Setup(m => m.Parse(describeLines1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeLines2)).Returns(changeset2);
            this.describeParserMock.Setup(m => m.Parse(describeLines3)).Returns(changeset3);

            this.processor.Extract();
            var result = this.output;

            Assert.Equal(2, result.Count);
            Assert.Equal(2, result[new DateTime(2018, 07, 05)].Count);
            Assert.Single(result[new DateTime(2018, 07, 06)]);

            var dailyCodeChurn = result[new DateTime(2018, 07, 05)]["File1.cs"];
            Assert.Equal("2018/07/05 00:00:00", dailyCodeChurn.Timestamp);
            Assert.Equal("File1.cs", dailyCodeChurn.FileName);
            Assert.Equal(2, dailyCodeChurn.Added);
            Assert.Equal(4, dailyCodeChurn.Deleted);
            Assert.Equal(20, dailyCodeChurn.ChangesBefore);
            Assert.Equal(10, dailyCodeChurn.ChangesAfter);
            Assert.Equal(2, dailyCodeChurn.NumberOfChanges);

            dailyCodeChurn = result[new DateTime(2018, 07, 05)]["File2.cs"];
            Assert.Equal("2018/07/05 00:00:00", dailyCodeChurn.Timestamp);
            Assert.Equal("File2.cs", dailyCodeChurn.FileName);
            Assert.Equal(1, dailyCodeChurn.Added);
            Assert.Equal(2, dailyCodeChurn.Deleted);
            Assert.Equal(10, dailyCodeChurn.ChangesBefore);
            Assert.Equal(5, dailyCodeChurn.ChangesAfter);
            Assert.Equal(1, dailyCodeChurn.NumberOfChanges);

            dailyCodeChurn = result[new DateTime(2018, 07, 06)]["File1.cs"];
            Assert.Equal("2018/07/06 00:00:00", dailyCodeChurn.Timestamp);
            Assert.Equal("File1.cs", dailyCodeChurn.FileName);
            Assert.Equal(1, dailyCodeChurn.Added);
            Assert.Equal(2, dailyCodeChurn.Deleted);
            Assert.Equal(10, dailyCodeChurn.ChangesBefore);
            Assert.Equal(5, dailyCodeChurn.ChangesAfter);
            Assert.Equal(1, dailyCodeChurn.NumberOfChanges);
        }

        [Fact]
        public void WhenProcessingWithMultipleFilesShouldProcessOutputForMultipleFiles()
        {
            commandLineArgs.OutputType = OutputType.MultipleFile;
            var changesLines = new List<string>();
            var describeLines1 = new List<string>();
            var describeLines2 = new List<string>();

            var changeset1 = new PerforceChangeset();
            var changeset2 = new PerforceChangeset();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, changesLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(0, describeLines1));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(new Tuple<int, List<string>>(0, describeLines2));
            this.changesParserMock.Setup(m => m.Parse(changesLines)).Returns(new List<int>() { 1, 2 });
            this.describeParserMock.Setup(m => m.Parse(describeLines1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeLines2)).Returns(changeset2);

            this.processor.Extract();

            this.outputProcessorMock.Verify(m => m.ProcessOutput(OutputType.MultipleFile, It.IsAny<string>(), It.IsAny<Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>()), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldLogProgressEvery60Seconds()
        {
            var changesLines = new List<string>();
            var describeLines1 = new List<string>();
            var describeLines2 = new List<string>();

            var changeset1 = new PerforceChangeset();
            var changeset2 = new PerforceChangeset();

            this.processWrapperMock.Setup(m => m.Invoke("changes", "commandline")).Returns(new Tuple<int, List<string>>(0, changesLines));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "1")).Returns(new Tuple<int, List<string>>(0, describeLines1));
            this.processWrapperMock.Setup(m => m.Invoke("describe", "2")).Returns(new Tuple<int, List<string>>(0, describeLines2));
            this.changesParserMock.Setup(m => m.Parse(changesLines)).Returns(new List<int>() { 1, 2 });
            this.describeParserMock.Setup(m => m.Parse(describeLines1)).Returns(changeset1);
            this.describeParserMock.Setup(m => m.Parse(describeLines2)).Returns(changeset2);

            this.stopWatchMock.SetupSequence(m => m.TotalSecondsElapsed()).Returns(10).Returns(70);

            this.processor.Extract();

            this.loggerMock.Verify(m => m.LogToConsole("Processed 1/2 changesets"), Times.Once());
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndbugDatabaseNotSetShouldThrow()
        {
            this.commandLineArgs = new P4ExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll",
                BugDatabaseOutputFile = "some/path/to/output/files"
            };

            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, null, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

            Action collect = () => processor.QueryBugDatabase();

            Assert.Throws<NullReferenceException>(collect);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndDllIsEmptyShouldReturn()
        {
            this.commandLineArgs = new P4ExtractCommandLineArgs()
            {
                BugDatabaseDLL = string.Empty
            };

            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, bugDatabseMock.Object, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

            processor.QueryBugDatabase();

            this.bugDatabseMock.Verify(b => b.ProcessBugDatabase(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndNoOutputFileShouldThrowException()
        {
            this.commandLineArgs = new P4ExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll"
            };

            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, bugDatabseMock.Object, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

            Action collect = () => processor.QueryBugDatabase();

            var exception = Assert.Throws<Exception>(collect);
            Assert.Equal("Dll specified without known output file", exception.Message);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndBugCacheNullShouldReturn()
        {
            this.commandLineArgs = new P4ExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll",
                BugDatabaseOutputFile = "some/path/to/output/files"
            };

            this.bugDatabseMock
                .Setup(b => b.ProcessBugDatabase(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns((Dictionary<DateTime, Dictionary<string, WorkItem>>)null);

            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, bugDatabseMock.Object, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

            processor.QueryBugDatabase();

            this.loggerMock.Verify(l => l.LogToConsole(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheShouldProcessOutput()
        {
            this.commandLineArgs = new P4ExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll",
                BugDatabaseOutputFile = "some/path/to/output/files"
            };

            this.bugDatabseMock
                .Setup(b => b.ProcessBugDatabase(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new Dictionary<DateTime, Dictionary<string, WorkItem>>());

            this.processor = new PerforceCodeChurnProcessor(processWrapperMock.Object, changesParserMock.Object, describeParserMock.Object, commandLineParserMock.Object, bugDatabseMock.Object, loggerMock.Object, stopWatchMock.Object, outputProcessorMock.Object, commandLineArgs);

            processor.QueryBugDatabase();

            this.outputProcessorMock
                .Verify(o => o.ProcessOutput(this.commandLineArgs.BugDatabaseOutputType, this.commandLineArgs.BugDatabaseOutputFile, It.IsAny<Dictionary<DateTime, Dictionary<string, WorkItem>>>()),
                Times.Once);
        }
    }
}