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

        private MemoryStream memoryStream;

        public GivenAGetPerforceChanges()
        {
            this.mockCommandLineParser = new Mock<ICommandLineParser>();
            mockCommandLineParser.Setup(m => m.ParseCommandLine("p4 command")).Returns(new Tuple<string, string>("p4", "command"));

            this.mockProcessWrapper = new Mock<IProcessWrapper>();
            memoryStream = new MemoryStream();
            mockProcessWrapper.Setup(m => m.Invoke("p4", "command")).Returns(memoryStream);

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
        public void WhenProcessingShouldParseChanges()
        {
            this.cmdlet.DoProcessRecord();
            mockChangesParser.Verify(m => m.Parse(memoryStream), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldWriteOutput()
        {
            var changes = new List<int>();
            mockChangesParser.Setup(m => m.Parse(memoryStream)).Returns(changes);

            this.cmdlet.DoProcessRecord();
            mockCmdlet.Verify(m => m.WriteObject(changes), Times.Once());
        }
    }
}
