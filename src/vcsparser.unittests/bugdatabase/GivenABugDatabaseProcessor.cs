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
        private Mock<ILogger> loggerMock;

        private BugDatabaseProcessor bugDatabaseProcessor;
        private ChangesetProcessor changesetProcessor;

        private string someDllPath;
        private IEnumerable<string> someDllArgs;

        public GivenABugDatabaseProcessor()
        {
            this.bugDatabaseProviderMock = new Mock<IBugDatabaseProvider>();
            this.bugDatabaseProviderMock.Setup(b => b.ProcessArgs(It.IsAny<IEnumerable<string>>())).Returns(0);
            this.bugDatabaseProviderMock.Setup(b => b.Process()).Returns(new WorkItemList
            {
                TotalWorkItems = 1,
                WorkItems = new List<WorkItem>()
                {
                    new WorkItem
                    {
                        ChangesetId = "SomeChangeSetId",
                        ClosedDate = new DateTime(2019, 04, 11),
                        WorkItemId = "1"
                    }
                }
            });

            this.bugDatabaseLoaderMock = new Mock<IBugDatabaseDllLoader>();
            this.bugDatabaseLoaderMock.Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>())).Returns(bugDatabaseProviderMock.Object);

            this.workItemConverterMock = new Mock<IWorkItemConverter>();
            this.workItemConverterMock.Setup((w) => w.Convert(It.IsAny<IEnumerable<WorkItem>>())).Returns(new List<IChangeset>()
            {
                null
            });

            this.webRequest = new Mock<IWebRequest>();
            this.loggerMock = new Mock<ILogger>();

            this.someDllPath = "some/path/to/dll";
            this.someDllArgs = new List<string>() { "--some", "dll", "-arguments" };
        }

        private void CreateBugDatabaseProcessor()
        {
            this.bugDatabaseProcessor = new BugDatabaseProcessor(this.bugDatabaseLoaderMock.Object, this.workItemConverterMock.Object, this.webRequest.Object);
            this.changesetProcessor = new ChangesetProcessor("", this.loggerMock.Object);
        }

        [Fact]
        public void WhenProcessingNoChangesetShouldExit()
        {
            this.CreateBugDatabaseProcessor();

            this.bugDatabaseProcessor.Process(null, someDllPath, someDllArgs);

            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessingNoDllPathProcessortShouldExit()
        {
            this.CreateBugDatabaseProcessor();

            this.bugDatabaseProcessor.Process(changesetProcessor, null, someDllArgs);

            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessingNoDllArgsShouldExit()
        {
            this.CreateBugDatabaseProcessor();

            this.bugDatabaseProcessor.Process(changesetProcessor, someDllPath, null);

            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessingDatabaseProviderNullShouldExit()
        {
            this.bugDatabaseLoaderMock.Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>())).Returns((IBugDatabaseProvider)null);

            this.CreateBugDatabaseProcessor();
            this.bugDatabaseProcessor.Process(changesetProcessor, someDllPath, someDllArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Never);
        }

        [Fact]
        public void WhenProcessingDatabaseDllLoadedShouldProcessWorkItems()
        {
            this.CreateBugDatabaseProcessor();
            this.bugDatabaseProcessor.Process(changesetProcessor, someDllPath, someDllArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Once);
            this.workItemConverterMock.Verify(b => b.Convert(It.IsAny<IEnumerable<WorkItem>>()), Times.Once);
        }
    }
}
