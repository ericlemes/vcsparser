using Moq;
using System;
using System.Collections.Generic;
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
        private Mock<IJsonParser<WorkItem>> workItemParser;
        private Mock<ILogger> loggerMock;

        private BugDatabaseProcessor bugDatabaseProcessor;
        private ChangesetProcessor changesetProcessor;

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
                                    ClosedDate = "2019-04-11 00:00:00",
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

            this.workItemParser = new Mock<IJsonParser<WorkItem>>();

            this.loggerMock = new Mock<ILogger>();

            this.someDllPath = "some/path/to/dll";
            this.someDllArgs = new List<string>() { "--some", "dll", "-arguments" };
        }

        private void CreateBugDatabaseProcessor()
        {
            this.bugDatabaseProcessor = new BugDatabaseProcessor(this.bugDatabaseLoaderMock.Object, this.workItemConverterMock.Object, this.webRequest.Object, this.fileSystemMock.Object, this.workItemParser.Object, this.loggerMock.Object);
            this.changesetProcessor = new ChangesetProcessor("", this.loggerMock.Object);
        }

        [Fact]
        public void WhenProcessBugDatabaseNoDllPathProcessortShouldExit()
        {
            this.CreateBugDatabaseProcessor();

            this.bugDatabaseProcessor.ProcessBugDatabase(null, someDllArgs);

            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessBugDatabaseNoDllArgsShouldExit()
        {
            this.CreateBugDatabaseProcessor();

            this.bugDatabaseProcessor.ProcessBugDatabase(someDllPath, null);

            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessBugDatabaseProviderNullShouldExit()
        {
            this.bugDatabaseLoaderMock.Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>())).Returns((IBugDatabaseProvider)null);

            this.CreateBugDatabaseProcessor();
            this.bugDatabaseProcessor.ProcessBugDatabase(someDllPath, someDllArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheChangesetProviderNullShouldExit()
        {
            this.CreateBugDatabaseProcessor();
            this.bugDatabaseProcessor.ProcessCache("", null);

            this.workItemConverterMock.Verify(b => b.Convert(It.IsAny<List<WorkItem>>()), Times.Never);
        }

        [Fact]
        public void WhenProcessCacheShouldProcessWorkItems()
        {
            this.CreateBugDatabaseProcessor();
            this.bugDatabaseProcessor.ProcessBugDatabase(someDllPath, someDllArgs);
            this.bugDatabaseProcessor.ProcessCache("some/path/to/cache", this.changesetProcessor);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Once);
            //this.workItemConverterMock.Verify(b => b.Convert(It.IsAny<List<WorkItem>>()), Times.Once);

            // TODO Fix this test
        }

        // Tests when reading in files
    }
}
