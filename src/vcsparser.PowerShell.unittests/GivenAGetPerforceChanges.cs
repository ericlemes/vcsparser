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
    public class GivenAGetPerforceChanges
    {
        private GetPerforceChanges cmdlet;

        private Mock<ICommandLineParser> mockCommandLineParser;

        private Mock<IProcessWrapper> mockProcessWrapper;

        private Mock<IChangesParser> mockChangesParser;

        private Mock<ICmdlet> mockCmdlet;

        private List<string> invokeLines;

        public GivenAGetPerforceChanges()
        {
            this.mockCommandLineParser = new Mock<ICommandLineParser>();
            mockCommandLineParser.Setup(m => m.ParseCommandLine("p4 command")).Returns(new Tuple<string, string>("p4", "command"));

            this.mockProcessWrapper = new Mock<IProcessWrapper>();
            invokeLines = new List<string>();
            mockProcessWrapper.Setup(m => m.Invoke("p4", "command")).Returns(new Tuple<int, List<string>>(0, invokeLines));

            this.mockChangesParser = new Mock<IChangesParser>();
            this.mockCmdlet = new Mock<ICmdlet>();

            this.cmdlet = new GetPerforceChanges();
            this.cmdlet.InjectDependencies(mockCommandLineParser.Object, mockProcessWrapper.Object, mockChangesParser.Object, mockCmdlet.Object);

            this.cmdlet.ChangesCommand = "p4 command";


        }

        [Fact]
        public void WhenProcessingShouldParseCommandLine()
        {
            this.cmdlet.DoProcessRecord();
            mockCommandLineParser.Verify(m => m.ParseCommandLine("p4 command"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldInvokeP4Changes()
        {            
            this.cmdlet.DoProcessRecord();
            mockProcessWrapper.Verify(m => m.Invoke("p4", "command"), Times.Once());
        }

        [Fact]
        public void WhenProcessingInvokeP4NotZeroShouldThrow()
        {
            mockProcessWrapper.Setup(m => m.Invoke("p4", "command")).Returns(new Tuple<int, List<string>>(1, invokeLines));
            Action processRecord = () => this.cmdlet.DoProcessRecord();
            Assert.Throws<Exception>(processRecord);
        }

        [Fact]
        public void WhenProcessingShouldParseChanges()
        {
            this.cmdlet.DoProcessRecord();
            mockChangesParser.Verify(m => m.Parse(invokeLines), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldWriteOutput()
        {
            var changes = new List<int>();
            mockChangesParser.Setup(m => m.Parse(invokeLines)).Returns(changes);

            this.cmdlet.DoProcessRecord();
            mockCmdlet.Verify(m => m.WriteObject(changes), Times.Once());
        }
    }
}
