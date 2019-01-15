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

        public GivenAGetPerforceChangeset()
        {
            mockProcessWrapper = new Mock<IProcessWrapper>();
            mockCommandLineParser = new Mock<ICommandLineParser>();
            mockCommandLineParser.Setup(m => m.ParseCommandLine("p4 describe -ds 42")).
                Returns(new Tuple<string, string>("p4", "describe -ds 42"));
            mockDescribeParser = new Mock<IDescribeParser>();
            mockCmdLet = new Mock<ICmdlet>();

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
        public void WhenProcessingShouldExecuteDescribeCommand()
        {
            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();

            mockProcessWrapper.Verify(m => m.Invoke("p4", "describe -ds 42"));
        }

        [Fact]
        public void WhenProcessingShouldParseOutput()
        {
            var memoryStream = new MemoryStream();
            mockProcessWrapper.Setup(m => m.Invoke("p4", "describe -ds 42")).Returns(memoryStream);
            //var changeset = new PerforceChangeset();
            //mockDescribeParser.Setup(m => m.Parse(memoryStream)).Returns(changeset);

            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();
            mockDescribeParser.Verify(m => m.Parse(memoryStream), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldWriteObject()
        {
            var memoryStream = new MemoryStream();
            mockProcessWrapper.Setup(m => m.Invoke("p4", "describe -ds 42")).Returns(memoryStream);
            var changeset = new PerforceChangeset();
            mockDescribeParser.Setup(m => m.Parse(memoryStream)).Returns(changeset);

            cmdLet.Changeset = 42;
            cmdLet.DoProcessRecord();
            mockCmdLet.Verify(m => m.WriteObject(changeset), Times.Once());            
        }
        
    }
}
