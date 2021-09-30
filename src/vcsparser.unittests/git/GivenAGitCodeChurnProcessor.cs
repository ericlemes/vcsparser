using Moq;
using vcsparser.core;
using vcsparser.core.git;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using vcsparser.core.bugdatabase;

namespace vcsparser.unittests.git
{
    public class GivenAGitCodeChurnProcessor
    {
        private GitCodeChurnProcessor processor;

        private GitExtractCommandLineArgs args;

        private List<string> invokeLines;

        private Mock<IGitLogParser> gitLogParserMock;

        private Mock<IOutputProcessor> outputProcessorMock;

        private Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> processedOutput;

        private Mock<ICommandLineParser> commandLineParserMock;

        private Mock<IProcessWrapper> processWrapperMock;

        private Mock<IBugDatabaseProcessor> bugDatabaseMock;

        private Mock<ILogger> logger;

        public GivenAGitCodeChurnProcessor()
        {
            args = new GitExtractCommandLineArgs();
            args.GitLogCommand = "git log blah";
            args.OutputType = OutputType.SingleFile;
            args.OutputFile = "outputfile";

            invokeLines = new List<string>();

            gitLogParserMock = new Mock<IGitLogParser>();

            outputProcessorMock = new Mock<IOutputProcessor>();
            outputProcessorMock.Setup(m => m.ProcessOutput(args.OutputType, args.OutputFile, It.IsAny<Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>())).Callback<OutputType, string, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>(
                (outputType, outputFile, dict) =>
                {
                    this.processedOutput = dict;
                });

            gitLogParserMock.Setup(m => m.Parse(invokeLines)).Returns(new List<GitCommit>());

            commandLineParserMock = new Mock<ICommandLineParser>();
            commandLineParserMock.Setup(m => m.ParseCommandLine("git log blah")).Returns(new Tuple<string, string>("git", "log blah"));

            processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(m => m.Invoke("git", "log blah")).Returns(new Tuple<int, List<string>>(0, invokeLines));

            bugDatabaseMock = new Mock<IBugDatabaseProcessor>();

            this.logger = new Mock<ILogger>();

            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object, gitLogParserMock.Object, outputProcessorMock.Object, bugDatabaseMock.Object, logger.Object, args);
        }

        [Fact]
        public void WhenExtractingShouldInvokeCommandLine()
        {
            processor.Extract();

            processWrapperMock.Verify(m => m.Invoke("git", "log blah"), Times.Once());
        }

        [Fact]
        public void WhenExtractingShouldReturnExitCode()
        {
            var exitCode = processor.Extract();

            Assert.Equal(0, exitCode);
        }

        [Fact]
        public void WhenExtractingAndInvokeCommandLineNotZeroShouldReturnExitCode()
        {
            processWrapperMock.Setup(m => m.Invoke("git", "log blah")).Returns(new Tuple<int, List<string>>(1, invokeLines));

            var exitCode = processor.Extract();

            gitLogParserMock.Verify(m => m.Parse(invokeLines), Times.Never());
            Assert.Equal(1, exitCode);
        }

        [Fact]
        public void WhenExtractingShouldParseFile()
        {
            processor.Extract();
            gitLogParserMock.Verify(m => m.Parse(invokeLines), Times.Once());
        }

        [Fact]
        public void WhenExtractingShouldProcessOutput()
        {
            var changesets = new List<GitCommit>()
            {
                new GitCommit()
                {
                    Commiter = "author1",
                    CommiterDate = new DateTime(2018, 10, 2, 15, 30, 00),
                    ChangesetFileChanges = new List<FileChanges>()
                    {
                        new FileChanges()
                        {
                            Added = 10,
                            Deleted = 5,
                            FileName = "File1.cs"
                        },
                        new FileChanges(){
                            Added = 5,
                            Deleted = 1,
                            FileName = "File2.cs"
                        }
                    },
                    CommitHash = "SomeCommitHash"
                },
                new GitCommit()
                {
                    Commiter = "author1",
                    CommiterDate = new DateTime(2018, 10, 1, 12, 00, 00),
                    ChangesetFileChanges = new List<FileChanges>()
                    {
                        new FileChanges()
                        {
                            Added = 3,
                            Deleted = 2,
                            FileName = "File1.cs"
                        }
                    },
                    CommitHash = "SomeCommitHash"
                }
            };

            gitLogParserMock.Setup(m => m.Parse(invokeLines)).Returns(changesets);

            processor.Extract();
            Assert.Equal(2, processedOutput.Count);
        }

        [Fact]
        public void WhenExtractingAndHasBugRegexesShouldLogChangesetsWithBugs()
        {
            args = new GitExtractCommandLineArgs()
            {
                BugRegexes = "bug+",
                GitLogCommand = "git log blah",
                OutputType = OutputType.SingleFile,
                OutputFile = "outputfile"
            };
            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
                this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object, this.logger.Object, args);

            processor.Extract();

            this.logger.Verify(m => m.LogToConsole("Changesets with bugs: 0/0"));
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndbugDatabaseNotSetShouldThrow()
        {
            this.args = new GitExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll",
                BugDatabaseOutputFile = "some/path/to/output/files"
            };

            this.processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
                this.gitLogParserMock.Object, this.outputProcessorMock.Object, null, this.logger.Object, args);

            Action collect = () => processor.QueryBugDatabase();

            Assert.Throws<NullReferenceException>(collect);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndDllIsEmptyShouldReturn()
        {
            args = new GitExtractCommandLineArgs()
            {
                BugDatabaseDLL = string.Empty
            };

            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
               this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object, this.logger.Object, args);

            processor.QueryBugDatabase();

            this.bugDatabaseMock.Verify(b => b.ProcessBugDatabase(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheAndNoOutputFileShouldThrowException()
        {
            args = new GitExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll"
            };

            Action action = () => new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
                this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object, this.logger.Object, args);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal("Dll specified without known output file", exception.Message);
        }

        [Fact]
        public void WhenCollectingBugDatabaseCacheShouldProcessOutput()
        {
            args = new GitExtractCommandLineArgs()
            {
                BugDatabaseDLL = "some/path/to.dll",
                BugDatabaseOutputFile = "some/path/to/output/files"
            };

            this.bugDatabaseMock
                .Setup(b => b.ProcessBugDatabase(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new Dictionary<DateTime, Dictionary<string, WorkItem>>());

            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
               this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object, this.logger.Object, args);

            processor.QueryBugDatabase();

            this.outputProcessorMock
                .Verify(o => o.ProcessOutput(args.BugDatabaseOutputType, args.BugDatabaseOutputFile, It.IsAny<Dictionary<DateTime, Dictionary<string, WorkItem>>>()),
                Times.Once);
        }

        [Fact]
        public void WhenCrearingGitCodeChurnProcessorWithBugDatabaseDllAndNoOutputSpecifiedShouldThrowAnException()
        {
            var commandLineArgs = new GitExtractCommandLineArgs
            {
                BugDatabaseDLL = "some/path/to.dll"
            };

            Action action = () =>
            {
                new GitCodeChurnProcessor(this.commandLineParserMock.Object,
                    this.processWrapperMock.Object,
                    this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object,
                    this.logger.Object, commandLineArgs);
            };

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal("Dll specified without known output file", exception.Message);
        }

        [Fact]
        public void WhenUsingGitExtractToCosmosDbCommandLineArgsAndExtractingShouldLogChangesetsWithBugs()
        {
            var args = new GitExtractToCosmosDbCommandLineArgs()
            {
                GitLogCommand = "git log blah",
                BugDatabaseDLL = "some/path/to.dll",
                BugRegexes = "bug+"
            };

            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object,
                this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabaseMock.Object, this.logger.Object, args);

            processor.Extract();

            this.logger.Verify(m => m.LogToConsole("Changesets with bugs: 0/0"));
        }
    }
}