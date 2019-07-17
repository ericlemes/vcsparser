using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.p4;
using Xunit;

namespace vcsparser.PowerShell.unittests
{
    public class GivenAGetPerforceChangeset
    {
        private GetPerforceChangeset cmdLet;

        private Mock<IProcessWrapper> mockProcessWrapper;

        private Mock<ICommandLineParser> mockCommandLineParser;

        private Mock<IDescribeParser> mockDescribeParser;

        private Mock<ICmdlet> mockCmdLet;

        private List<string> invokeLines;

        public GivenAGetPerforceChangeset()
        {
            mockProcessWrapper = new Mock<IProcessWrapper>();
            mockCommandLineParser = new Mock<ICommandLineParser>();
            mockCommandLineParser.Setup(m => m.ParseCommandLine("p4 describe -ds 42")).
                Returns(new Tuple<string, string>("p4", "describe -ds 42"));
            mockDescribeParser = new Mock<IDescribeParser>();
            mockCmdLet = new Mock<ICmdlet>();

            invokeLines = new List<string>();
            mockProcessWrapper.Setup(m => m.Invoke("p4", "describe -ds 42")).Returns(new Tuple<int, List<string>>(0, invokeLines));

            cmdLet = new GetPerforceChangeset();
            cmdLet.InjectDependencies(mockDescribeParser.Object, mockProcessWrapper.Object, mockCommandLineParser.Object,
                mockCmdLet.Object);
        }

        [Fact]
        public void WhenConstructingShouldInitializeDescribeCommand()
        {            
            Assert.Equal("p4 describe -ds {0}", cmdLet.DescribeCommand);
        }

        [Fact]
        public void WhenProcessingInvokeP4NotZeroShouldThrow()
        {
            mockCommandLineParser.Setup(m => m.ParseCommandLine("p4 describe -ds 0")).
                Returns(new Tuple<string, string>("p4", "describe -ds 0"));
            mockProcessWrapper.Setup(m => m.Invoke("p4", "describe -ds 0")).Returns(new Tuple<int, List<string>>(1, invokeLines));
            Action processRecord = () => cmdLet.DoProcessRecord();
            Assert.Throws<Exception>(processRecord);
        }

        [Fact]
        public void WhenProcessingShouldExecuteDescribeCommand()
        {
            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();

            mockProcessWrapper.Verify(m => m.Invoke("p4", "describe -ds 42"), Times.Once);
        }

        [Fact]
        public void WhenProcessingShouldParseOutput()
        {
            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();
            mockDescribeParser.Verify(m => m.Parse(invokeLines), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldWriteObject()
        {
            var changeset = new PerforceChangeset();
            mockDescribeParser.Setup(m => m.Parse(invokeLines)).Returns(changeset);

            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();
            mockCmdLet.Verify(m => m.WriteObject(changeset), Times.Once());            
        }
        
    }
}
