using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.git;
using Xunit;

namespace vcsparser.PowerShell.unittests
{
    public class GivenAGetGitLog
    {
        private Mock<ICmdlet> mockCmdlet;

        private GetGitLog cmdlet;

        private Mock<ICommandLineParser> mockCommandLineParser;

        private Mock<IProcessWrapper> mockProcessWrapper;
        private MemoryStream memoryStream;

        private Mock<IGitLogParser> mockGitLogParser;

        public GivenAGetGitLog()
        {
            this.mockCmdlet = new Mock<ICmdlet>();
            this.mockCommandLineParser = new Mock<ICommandLineParser>();
            mockCommandLineParser.Setup(m => m.ParseCommandLine("git log")).Returns(new Tuple<string, string>("git", "log"));

            this.mockProcessWrapper = new Mock<IProcessWrapper>();
            memoryStream = new MemoryStream();
            mockProcessWrapper.Setup(m => m.Invoke("git", "log", "workingdir")).Returns(memoryStream);

            this.mockGitLogParser = new Mock<IGitLogParser>();
            mockGitLogParser.Setup(m => m.Parse(this.memoryStream)).Returns(new List<GitCommit>());

            this.cmdlet = new GetGitLog();
            this.cmdlet.GitLogCommand = "git log";
            this.cmdlet.WorkingDirectory = "workingdir";
            this.cmdlet.InjectDependencies(mockCommandLineParser.Object, mockProcessWrapper.Object,
                mockGitLogParser.Object, mockCmdlet.Object);
        }

        [Fact]
        public void WhenProcessingShouldParseCommandLine()
        {
            this.cmdlet.DoProcessRecord();
            mockCommandLineParser.Verify(m => m.ParseCommandLine("git log"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldInvokeGitLog()
        {
            this.cmdlet.DoProcessRecord();
            mockProcessWrapper.Verify(m => m.Invoke("git", "log", "workingdir"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldParseChanges()
        {
            this.cmdlet.DoProcessRecord();
            mockGitLogParser.Verify(m => m.Parse(memoryStream), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldWriteOutput()
        {
            var gitLog = new List<GitCommit>();
            mockGitLogParser.Setup(m => m.Parse(memoryStream)).Returns(gitLog);

            this.cmdlet.DoProcessRecord();
            mockCmdlet.Verify(m => m.WriteObject(gitLog), Times.Once());
        }
    }
}
