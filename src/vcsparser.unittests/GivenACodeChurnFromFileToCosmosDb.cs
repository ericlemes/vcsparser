using System;
using Moq;
using System.Collections.Generic;
using vcsparser.core;
using vcsparser.core.Database;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenACodeChurnFromFileToCosmosDb
    {
        private Mock<ILogger> loggerMock;
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IOutputProcessor> outputProcessorMock;
        private Mock<IJsonListParser<DailyCodeChurn>> jsonListParserMock;

        private readonly string somePath = "C:/some/path";
        private readonly string someCosmosDbProjectName = "SomeProjectName";
        private readonly string someFileName = "SomeFileName";
        private readonly string someCosmosTimestamp = "2017/05/30 00:00:00";

        private DataFromFileToCosmosDb<DailyCodeChurn> sut;

        public GivenACodeChurnFromFileToCosmosDb()
        {
            loggerMock = new Mock<ILogger>();
            fileSystemMock = new Mock<IFileSystem>();
            outputProcessorMock = new Mock<IOutputProcessor>();
            jsonListParserMock = new Mock<IJsonListParser<DailyCodeChurn>>();

            sut = new DataFromFileToCosmosDb<DailyCodeChurn>(loggerMock.Object, fileSystemMock.Object, outputProcessorMock.Object, jsonListParserMock.Object, somePath, someCosmosDbProjectName);
        }

        [Fact]
        public void WhenExtractShouldCallProcessorWithCorrectParameters()
        {
            var fileMock = new Mock<IFile>();
            fileMock
                .Setup(f => f.FileName)
                .Returns("SomeFile.json");

            this.jsonListParserMock.Setup(w => w.ParseFile(It.IsAny<string>())).Returns(new List<DailyCodeChurn>()
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
    }
}
