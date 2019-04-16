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

        private MemoryStream memoryStream;

        private Mock<IGitLogParser> gitLogParserMock;

        private Mock<IOutputProcessor> outputProcessorMock;

        private Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> processedOutput;

        private Mock<ICommandLineParser> commandLineParserMock;

        private Mock<IProcessWrapper> processWrapperMock;

        private Mock<IBugDatabaseProcessor> bugDatabseMock;

        private Mock<ILogger> logger;

        public GivenAGitCodeChurnProcessor()
        {
            args = new GitExtractCommandLineArgs();
            args.GitLogCommand = "git log blah";
            args.OutputType = OutputType.SingleFile;
            args.OutputFile = "outputfile";

            memoryStream = new MemoryStream();

            gitLogParserMock = new Mock<IGitLogParser>();

            outputProcessorMock = new Mock<IOutputProcessor>();
            outputProcessorMock.Setup(m => m.ProcessOutput(args.OutputType, args.OutputFile, It.IsAny<Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>())).Callback<OutputType, string, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>(
                (outputType, outputFile, dict) =>
                {
                    this.processedOutput = dict;
                });

            gitLogParserMock.Setup(m => m.Parse(this.memoryStream)).Returns(new List<GitCommit>());

            commandLineParserMock = new Mock<ICommandLineParser>();
            commandLineParserMock.Setup(m => m.ParseCommandLine("git log blah")).Returns(new Tuple<string, string>("git", "log blah"));

            processWrapperMock = new Mock<IProcessWrapper>();
            processWrapperMock.Setup(m => m.Invoke("git", "log blah")).Returns(this.memoryStream);

            bugDatabseMock = new Mock<IBugDatabaseProcessor>();

            this.logger = new Mock<ILogger>();

            processor = new GitCodeChurnProcessor(this.commandLineParserMock.Object, this.processWrapperMock.Object, gitLogParserMock.Object, outputProcessorMock.Object, bugDatabseMock.Object, logger.Object, args);            
        }

        [Fact]
        public void WhenExtractingShouldInvokeCommandLine()
        {
            processor.Extract();
            
            processWrapperMock.Verify(m => m.Invoke("git", "log blah"), Times.Once());            
        }

        [Fact]
        public void WhenExtractingShouldParseFile()
        {
            processor.Extract();
            gitLogParserMock.Verify(m => m.Parse(this.memoryStream), Times.Once());
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
                    }
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
                    }
                }
            };

            gitLogParserMock.Setup(m => m.Parse(this.memoryStream)).Returns(changesets);
            
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
                this.gitLogParserMock.Object, this.outputProcessorMock.Object, this.bugDatabseMock.Object, this.logger.Object, args);

            processor.Extract();

            this.logger.Verify(m => m.LogToConsole("Changesets with bugs: 0/0"));
        }
    }
}
