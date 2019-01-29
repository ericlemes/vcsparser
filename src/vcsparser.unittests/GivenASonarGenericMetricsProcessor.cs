using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenASonarGenericMetricsProcessor
    {
        private Mock<IFileSystem> fileSystemMock;

        private SonarGenericMetricsProcessor processor;

        private SonarGenericMetricsCommandLineArgs commandLineArgs;

        private Mock<IDailyCodeChurnParser> parserMock;

        private List<IMeasureConverter> measureConverters;

        private Mock<IMeasureConverter> measureConverter1Mock;
        private Mock<IMeasureConverter> measureConverter2Mock;

        private List<DailyCodeChurn> dailyCodeChurn1;
        private List<DailyCodeChurn> dailyCodeChurn2;

        private Mock<IJsonExporter> jsonExporterMock;        

        public GivenASonarGenericMetricsProcessor()
        {
            this.commandLineArgs = new SonarGenericMetricsCommandLineArgs();
            this.commandLineArgs.InputDir = "inputDir";

            this.fileSystemMock = new Mock<IFileSystem>();
            this.parserMock = new Mock<IDailyCodeChurnParser>();            

            var file1Mock = new Mock<IFile>();
            file1Mock.Setup(m => m.FileName).Returns("file1");
            var file2Mock = new Mock<IFile>();
            file2Mock.Setup(m => m.FileName).Returns("file2");
            var filesMock = new List<IFile>()
            {
                file1Mock.Object,
                file2Mock.Object
            };
            this.fileSystemMock.Setup(m => m.GetFiles(this.commandLineArgs.InputDir, "*.json")).Returns(filesMock);

            measureConverter1Mock = new Mock<IMeasureConverter>();
            measureConverter2Mock = new Mock<IMeasureConverter>();

            this.measureConverters = new List<IMeasureConverter>()
            {
                measureConverter1Mock.Object,
                measureConverter2Mock.Object
            };

            this.dailyCodeChurn1 = new List<DailyCodeChurn>()
            {
                new DailyCodeChurn(),
                new DailyCodeChurn()
            };
            this.dailyCodeChurn2 = new List<DailyCodeChurn>()
            {
                new DailyCodeChurn()
            };

            this.parserMock.Setup(m => m.ParseFile("file1")).Returns(dailyCodeChurn1);
            this.parserMock.Setup(m => m.ParseFile("file2")).Returns(dailyCodeChurn2);

            this.jsonExporterMock = new Mock<IJsonExporter>();

            this.processor = new SonarGenericMetricsProcessor(this.fileSystemMock.Object, this.parserMock.Object, this.measureConverters, this.jsonExporterMock.Object, new Mock<ILogger>().Object);
        }

        [Fact]
        public void WhenProcessingShouldReadJsonFilesFromInputDirectory()
        {
            processor.Process(this.commandLineArgs);
            fileSystemMock.Verify(m => m.GetFiles("inputDir", "*.json"), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldParseJsonFiles()
        {
            processor.Process(this.commandLineArgs);

            this.parserMock.Verify(m => m.ParseFile("file1"), Times.Once());
            this.parserMock.Verify(m => m.ParseFile("file2"), Times.Once());
        }

        [Fact]
        public void WhenRunningThisShouldFail()
        {
            Assert.True(false);
        }

        [Fact]
        public void WhenProcessingShouldProcessMeasureConverters()
        {
            processor.Process(this.commandLineArgs);

            this.measureConverter1Mock.Verify(m => m.Process(dailyCodeChurn1[0], It.IsAny<SonarMeasuresJson>()), Times.Once());
            this.measureConverter1Mock.Verify(m => m.Process(dailyCodeChurn1[1], It.IsAny<SonarMeasuresJson>()), Times.Once());
            this.measureConverter1Mock.Verify(m => m.Process(dailyCodeChurn2[0], It.IsAny<SonarMeasuresJson>()), Times.Once());
            this.measureConverter2Mock.Verify(m => m.Process(dailyCodeChurn1[0], It.IsAny<SonarMeasuresJson>()), Times.Once());
            this.measureConverter2Mock.Verify(m => m.Process(dailyCodeChurn1[1], It.IsAny<SonarMeasuresJson>()), Times.Once());
            this.measureConverter2Mock.Verify(m => m.Process(dailyCodeChurn2[0], It.IsAny<SonarMeasuresJson>()), Times.Once());
        }

        [Fact]
        public void WhenProcessingShouldExportJson()
        {
            processor.Process(this.commandLineArgs);
            this.jsonExporterMock.Verify(m => m.Export(It.IsAny<SonarMeasuresJson>(), this.commandLineArgs.OutputFile));
        }
    }
}
