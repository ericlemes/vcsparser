using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.PowerShell.unittests
{
    public class GivenAStartProcessSync
    {
        StartProcessSync startProcess;
        Mock<IProcessWrapper> mockProcessWrapper;
        Mock<ICommandLineParser> mockCommandLineParser;
        Mock<ILogger> mockLogger;
        Mock<ICmdlet> mockCmdlet;
        MemoryStream memoryStream;

        public GivenAStartProcessSync()
        {
            mockProcessWrapper = new Mock<IProcessWrapper>();
            mockCommandLineParser = new Mock<ICommandLineParser>();
            mockLogger = new Mock<ILogger>();
            mockCmdlet = new Mock<ICmdlet>();
            memoryStream = new MemoryStream();

            startProcess = new StartProcessSync();
            startProcess.WorkingDirectory = "workingDir";
            startProcess.CommandLine = "cmd args";
            startProcess.InjectDependencies(mockCmdlet.Object, mockProcessWrapper.Object, mockCommandLineParser.Object, mockLogger.Object);

            mockCommandLineParser.Setup(m => m.ParseCommandLine("cmd args")).Returns(new Tuple<string, string>("cmd", "args"));
            mockProcessWrapper.Setup(m => m.Invoke("cmd", "args", It.IsAny<OutputLineDelegate>())).Returns(0);
        }

        [Fact]
        public void WhenProcessRecordShouldInvokeProcessWrapper()
        {
            mockProcessWrapper.Setup(m => m.Invoke("cmd", "args", "workingDir", It.IsAny<OutputLineDelegate>())).Callback<string, string, string, OutputLineDelegate>(
                (string cmd, string args, string workingDir, OutputLineDelegate outputLineDelegate) => {
                    outputLineDelegate.Invoke("line 1");
                    outputLineDelegate.Invoke("line 2");
                    outputLineDelegate.Invoke("line 3");                    
                }).Returns(-1);

            startProcess.DoProcessRecord();
            mockCmdlet.Verify(m => m.WriteObject(-1), Times.Once());
            mockLogger.Verify(m => m.LogToConsole("line 1"), Times.Once());
            mockLogger.Verify(m => m.LogToConsole("line 2"), Times.Once());
            mockLogger.Verify(m => m.LogToConsole("line 3"), Times.Once());
        }
    }
}
