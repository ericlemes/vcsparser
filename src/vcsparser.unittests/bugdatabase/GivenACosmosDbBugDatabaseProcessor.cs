using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenACosmosDbBugDatabaseProcessor
    {
        private const string SomeProjectName = "Some-Project-Name";
        private string someDllPath;

        private Mock<IBugDatabaseProvider> bugDatabaseProviderMock;
        private Mock<IBugDatabaseDllLoader> bugDatabaseLoaderMock;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IDataDocumentRepository> dataDocumentRepository;
        private Mock<IChangesetProcessor> changesetProcessorMock;

        private Mock<IWebRequest> webRequest;
        private Mock<ILogger> loggerMock;
        private IEnumerable<string> someDllArgs;

        private CosmosDbBugDatabaseProcessor sut;


        public GivenACosmosDbBugDatabaseProcessor()
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

            this.changesetProcessorMock = new Mock<IChangesetProcessor>();
            this.changesetProcessorMock.Setup(c => c.WorkItemCache).Returns(new Dictionary<string, List<WorkItem>>());
            this.webRequest = new Mock<IWebRequest>();
            this.bugDatabaseLoaderMock = new Mock<IBugDatabaseDllLoader>();
            this.bugDatabaseLoaderMock
                .Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()))
                .Returns(bugDatabaseProviderMock.Object);
            this.fileSystemMock = new Mock<IFileSystem>();
            this.loggerMock = new Mock<ILogger>();
            this.dataDocumentRepository = new Mock<IDataDocumentRepository>();
            this.someDllArgs = new List<string>() { "--some", "dll", "-arguments" };
            this.someDllPath = "some/path/to/dll";

            this.sut = new CosmosDbBugDatabaseProcessor(bugDatabaseLoaderMock.Object, fileSystemMock.Object, webRequest.Object, loggerMock.Object, dataDocumentRepository.Object, SomeProjectName);
        }


        [Fact]
        public void WhenProcessBugDatabaseNoDllPathProcessortShouldExit()
        {
            this.fileSystemMock.Setup(f => f.GetFullPath(It.IsAny<string>())).Throws<ArgumentNullException>();
            Action action = () => this.sut.ProcessBugDatabase(null, someDllArgs);

            Assert.Throws<ArgumentNullException>(action);
            this.bugDatabaseLoaderMock.Verify(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()), Times.Never);
        }

        [Fact]
        public void WhenProcessBugDatabaseProviderNullShouldExit()
        {
            this.bugDatabaseLoaderMock
                .Setup(b => b.Load(It.IsAny<string>(), It.IsAny<IEnumerable<string>>(), It.IsAny<IWebRequest>()))
                .Throws(new Exception("Some Exception!"));

            Action action = () => this.sut.ProcessBugDatabase(someDllPath, someDllArgs);

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

            this.sut.ProcessBugDatabase(someDllPath, someDllArgs);

            this.bugDatabaseProviderMock.Verify(b => b.Process(), Times.Once);
        }

        [Fact]
        public void WhenProcessCacheFilesFoundNullChangesetShouldSkipIt()
        {
            var fileMock = new Mock<IFile>();
            fileMock.Setup(f => f.FileName).Returns("SomeFile.json");

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            dataDocumentRepository.Setup(x => x.GetAllDocumentsByProjectAndDocumentType<WorkItem>(SomeProjectName, DocumentType.BugDatabase))
                .Returns(new List<CosmosDataDocument<WorkItem>>()
            {
                new CosmosDataDocument<WorkItem>
                {
                    Data = new List<WorkItem>
                    {
                        new WorkItem
                        {
                            ChangesetId = "SameChangeSetId",
                            ClosedDate =  new DateTime(2019, 04, 11),
                            WorkItemId = "1"
                        }
                    }
                },

                new CosmosDataDocument<WorkItem>
                {
                    Data = new List<WorkItem>
                    {
                        new WorkItem
                        {
                            ChangesetId = null,
                            ClosedDate =  new DateTime(2019, 04, 11),
                            WorkItemId = "2"
                        }
                    }
                },
                new CosmosDataDocument<WorkItem>
                {
                    Data = new List<WorkItem>
                    {
                        new WorkItem
                        {
                            ChangesetId = "SameChangeSetId",
                            ClosedDate =  new DateTime(2019, 04, 11),
                            WorkItemId = "3"
                        }
                    }
                },

            });

            this.sut.ProcessCache(this.changesetProcessorMock.Object);

            Assert.Single(this.changesetProcessorMock.Object.WorkItemCache);
        }
    }
}
