using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    internal class NotAnImplementationOfIBugDatabaseProvider { }

    public class GivenABugDatabaseDllLoader
    {
        private Mock<_Assembly> assemblyMock;
        private Mock<IBugDatabaseProvider> bugDatabaseProviderMock;
        private Mock<IBugDatabaseFactory> bugDatabaseFactoryMock;
        private Mock<IWebRequest> webRequestMock;
        private Mock<ILogger> loggerMock;

        private IBugDatabaseDllLoader bugDatabaseDllLoader;

        private string dllPath;
        private IEnumerable<string> dllArgs;

        public GivenABugDatabaseDllLoader()
        {
            this.assemblyMock = new Mock<_Assembly>();
            this.assemblyMock.Setup((a) => a.GetExportedTypes()).Returns(new Type[] { typeof(IBugDatabaseProvider) });

            this.bugDatabaseProviderMock = new Mock<IBugDatabaseProvider>();
            this.bugDatabaseProviderMock.Setup((p) => p.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(0);

            this.bugDatabaseFactoryMock = new Mock<IBugDatabaseFactory>();
            this.bugDatabaseFactoryMock.Setup((f) => f.LoadFile(It.IsAny<string>())).Returns(this.assemblyMock.Object);
            this.bugDatabaseFactoryMock.Setup((f) => f.CreateInstance(It.IsAny<Type>())).Returns(this.bugDatabaseProviderMock.Object);

            this.webRequestMock = new Mock<IWebRequest>();

            this.loggerMock = new Mock<ILogger>();

            this.bugDatabaseDllLoader = new BugDatabaseDllLoader(this.loggerMock.Object, this.bugDatabaseFactoryMock.Object);

            this.dllPath = "some/path/to.dll";
            this.dllArgs = new List<string>() { "--some", "dll", "-args" };
        }

        [Fact]
        public void WhenLoadFileThrownThenReturnNull()
        {
            this.bugDatabaseFactoryMock.Setup((f) => f.LoadFile(It.IsAny<string>())).Returns(() => throw new Exception("Some Exception!"));

            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.Null(provider);
            this.loggerMock.Verify((l) => l.LogToConsole($"Error loading Dll. Some Exception!"), Times.Once);
        }

        [Fact]
        public void WhenNoTypesFoundThenReturnNull()
        {
            this.assemblyMock.Setup((a) => a.GetExportedTypes()).Returns(new Type[] { });

            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.Null(provider);
            this.loggerMock.Verify((l) => l.LogToConsole($"Dll must contain a public implementation of '{typeof(IBugDatabaseProvider)}'"), Times.Once);
        }

        [Fact]
        public void WhenNoValidTypesFoundThenReturnNull()
        {
            this.assemblyMock.Setup((a) => a.GetExportedTypes()).Returns(new Type[] { typeof(NotAnImplementationOfIBugDatabaseProvider) });

            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.Null(provider);
            this.loggerMock.Verify((l) => l.LogToConsole($"Dll must contain a public implementation of '{typeof(IBugDatabaseProvider)}'"), Times.Once);
        }

        [Fact]
        public void WhenMultipleValidTypesFoundThenReturnNull()
        {
            this.assemblyMock.Setup((a) => a.GetExportedTypes()).Returns(new Type[] { typeof(IBugDatabaseProvider), typeof(IBugDatabaseProvider) });

            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.Null(provider);
            this.loggerMock.Verify((l) => l.LogToConsole($"Dll can only contain one public implementation of '{typeof(IBugDatabaseProvider)}'. Found 2"), Times.Once);
        }

        [Fact]
        public void WhenFaildToProcessArgsThenReturnNull()
        {
            this.bugDatabaseProviderMock.Setup((p) => p.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(1);

            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.Null(provider);
            this.loggerMock.Verify((l) => l.LogToConsole("Unable to parse Dll arguments. Check requirements"), Times.Once);
        }

        [Fact]
        public void WhenValidDllThenReturnBugDatabaseProvider()
        {
            var provider = this.bugDatabaseDllLoader.Load(this.dllPath, this.dllArgs, this.webRequestMock.Object);

            Assert.NotNull(provider);
            Assert.IsAssignableFrom<IBugDatabaseProvider>(provider);
        }
    }
}
