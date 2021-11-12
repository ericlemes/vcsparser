using System;
using Moq;
using System.Collections.Generic;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenADataFromFileToCosmosDb
    {
        private Mock<ILogger> loggerMock;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IOutputProcessor> outputProcessorMock;
        private Mock<IJsonListParser<DailyCodeChurn>> jsonListParserMockDailyCodeChurn;
        private Mock<IJsonListParser<WorkItem>> jsonListParserMockWorkItem;

        private readonly string somePath = "C:/some/path";
        private readonly string someCosmosDbProjectName = "SomeProjectName";
        private readonly string someFileName = "SomeFileName";
        private readonly string someCosmosTimestamp = "2017/05/30 00:00:00";
        private readonly string changesetId = "some-changeset-id";

        private DataFromFileToCosmosDb<DailyCodeChurn> sut;

        public GivenADataFromFileToCosmosDb()
        {
            loggerMock = new Mock<ILogger>();
            fileSystemMock = new Mock<IFileSystem>();
            outputProcessorMock = new Mock<IOutputProcessor>();
            jsonListParserMockDailyCodeChurn = new Mock<IJsonListParser<DailyCodeChurn>>();
            jsonListParserMockWorkItem = new Mock<IJsonListParser<WorkItem>>();

            sut = new DataFromFileToCosmosDb<DailyCodeChurn>(loggerMock.Object, fileSystemMock.Object, outputProcessorMock.Object, jsonListParserMockDailyCodeChurn.Object, somePath, someCosmosDbProjectName);
        }

        [Fact]
        public void WhenExtractShouldCallProcessorWithCorrectParameters()
        {
            var fileMock = new Mock<IFile>();
            fileMock
                .Setup(f => f.FileName)
                .Returns("SomeFile.json");

            this.jsonListParserMockDailyCodeChurn.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<DailyCodeChurn>()
            {
                new DailyCodeChurn
                {
                    Timestamp = someCosmosTimestamp,
                    FileName = someFileName
                }
            });

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            sut.Extract();

            outputProcessorMock.Verify(x => x.ProcessOutput(OutputType.CosmosDb, someCosmosDbProjectName, It.Is<Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>>(item => item.Count == 1)));
        }

        [Fact]
        public void WhenExtractWorkItemShouldCallProcessorWithCorrectParameters()
        {
            var sutWorkItem = new DataFromFileToCosmosDb<WorkItem>(loggerMock.Object, fileSystemMock.Object, outputProcessorMock.Object, jsonListParserMockWorkItem.Object, somePath, someCosmosDbProjectName);

            var fileMock = new Mock<IFile>();
            fileMock
                .Setup(f => f.FileName)
                .Returns("SomeFile.json");

            this.jsonListParserMockWorkItem.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<WorkItem>()
            {
                new WorkItem
                {
                    ClosedDate = DateTime.Now,
                    ChangesetId = changesetId
                }
            });

            this.fileSystemMock
                .Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new List<IFile>() { fileMock.Object });

            sutWorkItem.Extract();

            outputProcessorMock.Verify(x => x.ProcessOutput(OutputType.CosmosDb, someCosmosDbProjectName, It.Is<Dictionary<DateTime, Dictionary<string, WorkItem>>>(item => item.Count == 1)));
        }
    }
}
