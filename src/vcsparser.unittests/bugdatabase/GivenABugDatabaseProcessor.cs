using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenABugDatabaseProcessor
    {
        private Mock<IBugDatabaseProvider> bugDatabaseProviderMock;
        private Mock<IBugDatabaseDllLoader> bugDatabaseLoaderMock;
        private Mock<IWorkItemConverter> workItemConverterMock;
        private Mock<IWebRequest> webRequest;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IJsonListParser<WorkItem>> workItemParser;
        private Mock<ILogger> loggerMock;

        private BugDatabaseProcessor bugDatabaseProcessor;
        private Mock<IChangesetProcessor> changesetProcessorMock;

        private string someDllPath;
        private IEnumerable<string> someDllArgs;

        public GivenABugDatabaseProcessor()
        {
            this.bugDatabaseProviderMock = new Mock<IBugDatabaseProvider>();
            this.bugDatabaseProviderMock.Setup(b => b.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(0);
            this.bugDatabaseProviderMock.Setup(b => b.Process()).Returns(new Dictionary<DateTime, Dictionary<string, WorkItem>>()
            {
                    {
                        new DateTime(2019, 04, 11),
                        new Dictionary<string, WorkItem>()
                        {
                            {
                                "SomeChangeSetId",
                                new WorkItem
                                {
                                    ChangesetId = "SomeChangeSetId",
                                    ClosedDate =  new DateTime(2019, 04, 11),
                                    WorkItemId = "1"
                                }
                            }
                        }
                    }
            });

            this.bugDatabaseLoaderMock = new Mock<IBugDatabaseDllLoader>();
            this.bugDatabaseLoaderMock
                .Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()))
                .Returns(bugDatabaseProviderMock.Object);

            this.workItemConverterMock = new Mock<IWorkItemConverter>();
            this.workItemConverterMock
                .Setup((w) => w.Convert(It.IsAny<IEnumerable<WorkItem>>()))
                .Returns(new List<IChangeset>());

            this.webRequest = new Mock<IWebRequest>();

            this.fileSystemMock = new Mock<IFileSystem>();

            this.workItemParser = new Mock<IJsonListParser<WorkItem>>();

            this.loggerMock = new Mock<ILogger>();

            this.changesetProcessorMock = new Mock<IChangesetProcessor>();

            this.someDllPath = "some/path/to/dll";
            this.someDllArgs = new List<string>() { "--some", "dll", "-arguments" };
            this.bugDatabaseProcessor = new BugDatabaseProcessor(this.bugDatabaseLoaderMock.Object, this.workItemConverterMock.Object, this.webRequest.Object, this.fileSystemMock.Object, this.workItemParser.Object, this.loggerMock.Object);

        }

        [Fact]
        public void WhenProcessBugDatabaseNoDllPathProcessortShouldExit()
        {
            Action action = () => this.bugDatabaseProcessor.ProcessBugDatabase(null, someDllArgs);

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.StartsWith("Value cannot be null.", exception.Message);
            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessBugDatabaseProviderNullShouldExit()
        {
            this.bugDatabaseLoaderMock
                .Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()))
                .Throws(new Exception("Some Exception!"));

            Action action = () => this.bugDatabaseProcessor.ProcessBugDatabase(someDllPath, someDllArgs);

            var exception = Assert.Throws<Exception>(action);
            Assert.Equal("Some Exception!", exception.Message);
            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Never);
        }

        [Fact]
        public void WhenProcessBugDatabaseShouldProcess()
        {
            this.bugDatabaseLoaderMock
                .Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()))
                .Returns(bugDatabaseProviderMock.Object);

            this.bugDatabaseProcessor.ProcessBugDatabase(someDllPath, someDllArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheChangesetProviderNullShouldExit()
        {
            this.bugDatabaseProcessor.ProcessCache("", null);

            this.workItemConverterMock.Verify(b => b.Convert(It.IsAny<List<WorkItem>>()), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheCacheOutputShouldExit()
        {
            this.bugDatabaseProcessor.ProcessCache(null, this.changesetProcessorMock.Object);

            this.workItemConverterMock.Verify(b => b.Convert(It.IsAny<List<WorkItem>>()), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheShouldGetParentDirectory()
        {
            var cwd = Directory.GetCurrentDirectory();

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.fileSystemMock.Verify(f => f.GetFiles(cwd + "\\some\\path\\to", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundShouldProcessEachFile()
        {
            var fileMock = new Mock<IFile>();
            fileMock
                .SetupSequence(f => f.FileName)
                .Returns("SomeFile.json").Returns("SomeFile.json")
                .Returns("SomeOtherFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object, fileMock.Object });

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.loggerMock.Verify(l => l.LogToConsole($"Processing SomeFile.json"), Times.Once);
            this.loggerMock.Verify(l => l.LogToConsole($"Processing SomeOtherFile.json"), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheNoFilesFoundShouldNotProcessFiles()
        {
            this.fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new List<IFile>());

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.loggerMock.Verify(l => l.LogToConsole(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundShouldProcessChangesets()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            var changesetMock = new Mock<IChangeset>();

            this.workItemConverterMock
                .Setup(w => w.Convert(It.IsAny<IEnumerable<WorkItem>>()))
                .Returns(new List<IChangeset>() { changesetMock.Object });

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.changesetProcessorMock.Verify(c => c.ProcessBugDatabaseChangeset(changesetMock.Object), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundNoChangesetsShouldNotProcessChangesets()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });
            
            this.workItemConverterMock
                .Setup(w => w.Convert(It.IsAny<IEnumerable<WorkItem>>()))
                .Returns(new List<IChangeset>());

            this.bugDatabaseProcessor.ProcessCache("some\\path\\to\\cache", this.changesetProcessorMock.Object);

            this.changesetProcessorMock.Verify(c => c.ProcessBugDatabaseChangeset(It.IsAny<IChangeset>()), Times.Never);
        }
    }
}
