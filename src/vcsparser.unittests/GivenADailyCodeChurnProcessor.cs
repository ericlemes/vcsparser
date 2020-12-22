using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{    
    public class GivenADailyCodeChurnProcessor
    {
        private DailyCodeChurnProcessor processor;

        private DailyCodeChurnCommandLineArgs commandLineArgs;

        private Mock<IFileSystem> fileSystemMock;

        private Mock<IJsonListParser<DailyCodeChurn>> parserMock;

        private Mock<ILogger> loggerMock;

        private Mock<IExclusionsProcessor> exclusionsProcessorMock;

        private List<DailyCodeChurn> dailyCodeChurn1;
        private List<DailyCodeChurn> dailyCodeChurn2;

        private Mock<IJsonExporter> jsonExporter;

        public GivenADailyCodeChurnProcessor()
        {
            this.loggerMock = new Mock<ILogger>();
            this.fileSystemMock = new Mock<IFileSystem>();
            this.parserMock = new Mock<IJsonListParser<DailyCodeChurn>>();
            this.exclusionsProcessorMock = new Mock<IExclusionsProcessor>();
            this.jsonExporter = new Mock<IJsonExporter>();

            this.commandLineArgs = new DailyCodeChurnCommandLineArgs()
            {
                InputDir = "inputDir",
                FilePrefixToRemove = "filePrefix/",
                OutputFile = "OutputFile"
            };

            SetupFileSystem();
            SetupParser();

            this.exclusionsProcessorMock.Setup(m => m.IsExcluded("file1line1")).Returns(true);
            this.exclusionsProcessorMock.Setup(m => m.IsExcluded("file1line2")).Returns(false);
            this.exclusionsProcessorMock.Setup(m => m.IsExcluded("file2line1")).Returns(false);
            this.exclusionsProcessorMock.Setup(m => m.IsExcluded("file2line2")).Returns(false);

            this.processor = new DailyCodeChurnProcessor(fileSystemMock.Object, parserMock.Object, loggerMock.Object, exclusionsProcessorMock.Object, jsonExporter.Object);
        }

        private void SetupParser()
        {
            this.dailyCodeChurn1 = new List<DailyCodeChurn>()
            {
                new DailyCodeChurn(){
                    FileName = "filePrefix/file1line1",
                    Timestamp = "2020/12/21 00:00:00",
                    Added = 2,
                    AddedWithFixes = 1,
                    Deleted = 2,
                    DeletedWithFixes = 1,
                    ChangesBefore = 2,
                    ChangesBeforeWithFixes = 1,
                    ChangesAfter = 2,
                    ChangesAfterWithFixes = 1,
                    NumberOfChanges = 2,
                    NumberOfChangesWithFixes = 1,
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 1
                        }
                    },
                    BugDatabase = new DailyCodeChurnBugDatabase()
                    {
                        AddedInFixes = 1,
                        DeletedInFixes = 1,
                        NumberOfChangesInFixes = 1,
                        ChangesBeforeInFixes = 1,
                        ChangesAfterInFixes = 1
                    }
                },
                new DailyCodeChurn()
                {
                    FileName = "filePrefix/file1line2",
                    Timestamp = "2020/12/21 00:00:00",
                    Added = 2,
                    AddedWithFixes = 1,
                    Deleted = 2,
                    DeletedWithFixes = 1,
                    ChangesBefore = 2,
                    ChangesBeforeWithFixes = 1,
                    ChangesAfter = 2,
                    ChangesAfterWithFixes = 1,
                    NumberOfChanges = 2,
                    NumberOfChangesWithFixes = 1,
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 1
                        }
                    },
                    BugDatabase = new DailyCodeChurnBugDatabase()
                    {
                        AddedInFixes = 1,
                        DeletedInFixes = 1,
                        NumberOfChangesInFixes = 1,
                        ChangesBeforeInFixes = 1,
                        ChangesAfterInFixes = 1
                    }
                }
            };
            this.dailyCodeChurn2 = new List<DailyCodeChurn>()
            {
                new DailyCodeChurn()
                {
                    FileName = "filePrefix/file2line1",
                    Timestamp = "2020/12/21 00:00:00",
                    Added = 2,
                    AddedWithFixes = 1,
                    Deleted = 2,
                    DeletedWithFixes = 1,
                    ChangesBefore = 2,
                    ChangesBeforeWithFixes = 1,
                    ChangesAfter = 2,
                    ChangesAfterWithFixes = 1,
                    NumberOfChanges = 2,
                    NumberOfChangesWithFixes = 1,
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 1
                        }
                    },
                    BugDatabase = new DailyCodeChurnBugDatabase()
                    {
                        AddedInFixes = 1,
                        DeletedInFixes = 1,
                        NumberOfChangesInFixes = 1,
                        ChangesBeforeInFixes = 1,
                        ChangesAfterInFixes = 1
                    }                
                },
                new DailyCodeChurn()
                {
                    FileName = "filePrefix/file2line2",
                    Timestamp = "2020/12/22 00:00:00",
                    Added = 2,
                    AddedWithFixes = 1,
                    Deleted = 2,
                    DeletedWithFixes = 1,
                    ChangesBefore = 2,
                    ChangesBeforeWithFixes = 1,
                    ChangesAfter = 2,
                    ChangesAfterWithFixes = 1,
                    NumberOfChanges = 2,
                    NumberOfChangesWithFixes = 1,
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 1
                        }
                    },
                    BugDatabase = new DailyCodeChurnBugDatabase()
                    {
                        AddedInFixes = 1,
                        DeletedInFixes = 1,
                        NumberOfChangesInFixes = 1,
                        ChangesBeforeInFixes = 1,
                        ChangesAfterInFixes = 1
                    }                
                }
            };

            this.parserMock.Setup(m => m.ParseFile("file1")).Returns(dailyCodeChurn1);
            this.parserMock.Setup(m => m.ParseFile("file2")).Returns(dailyCodeChurn2);
        }

        private void SetupFileSystem()
        {
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
        public void WhenProcessingFilesShouldProcessExclusions()
        {
            processor.Process(this.commandLineArgs);
            this.exclusionsProcessorMock.Verify(m => m.IsExcluded("file1line1"));
            this.exclusionsProcessorMock.Verify(m => m.IsExcluded("file1line1"));
            this.exclusionsProcessorMock.Verify(m => m.IsExcluded("file2line1"));
            this.exclusionsProcessorMock.Verify(m => m.IsExcluded("file2line1"));
        }

        [Fact]
        public void WhenProcessingFilesShouldComputeExpectedValues()
        {
            IList<AggregatedDailyCodeChurn> result = null;
            this.jsonExporter.Setup(m => m.Export(It.IsAny<IList<AggregatedDailyCodeChurn>>(), this.commandLineArgs.OutputFile)).Callback<IList<AggregatedDailyCodeChurn>, string>(
                (IList<AggregatedDailyCodeChurn> p, string o) => { result = p; });
            processor.Process(this.commandLineArgs);
            Assert.Equal(2, result.Count);

            Assert.Equal("2020/12/21 00:00:00", result[0].Timestamp);
            Assert.Equal(4, result[0].Added);
            Assert.Equal(2, result[0].AddedInFixesVCS);
            Assert.Equal(2, result[0].AddedInFixesBugDB);
            Assert.Equal(4, result[0].Deleted);
            Assert.Equal(2, result[0].DeletedInFixesVCS);
            Assert.Equal(2, result[0].DeletedInFixesBugDB);
            Assert.Equal(4, result[0].ChangesBefore);
            Assert.Equal(2, result[0].ChangesBeforeInFixesVCS);
            Assert.Equal(2, result[0].ChangesBeforeInFixesBugDB);
            Assert.Equal(4, result[0].ChangesAfter);
            Assert.Equal(2, result[0].ChangesAfterInFixesVCS);
            Assert.Equal(2, result[0].ChangesAfterInFixesBugDB);
            Assert.Equal(4, result[0].NumberOfChanges);
            Assert.Equal(2, result[0].NumberOfChangesWithFixesVCS);
            Assert.Equal(2, result[0].NumberOfChangesWithFixesBugDB);
            Assert.Equal(2, result[0].Authors.Where(a => a.Author == "author1").First().NumberOfChanges);
            Assert.Equal(2, result[0].Authors.Where(a => a.Author == "author2").First().NumberOfChanges);
            Assert.Equal(2, result[0].Authors.Count);
            Assert.Equal(16, result[0].TotalLinesChanged);
            Assert.Equal(8, result[0].TotalLinesChangedInFixesBugDB);
            Assert.Equal(8, result[0].TotalLinesChangedInFixesVCS);

            Assert.Equal("2020/12/22 00:00:00", result[1].Timestamp);
            Assert.Equal(2, result[1].Added);
            Assert.Equal(1, result[1].AddedInFixesVCS);
            Assert.Equal(1, result[1].AddedInFixesBugDB);
            Assert.Equal(2, result[1].Deleted);
            Assert.Equal(1, result[1].DeletedInFixesVCS);
            Assert.Equal(1, result[1].DeletedInFixesBugDB);
            Assert.Equal(2, result[1].ChangesBefore);
            Assert.Equal(1, result[1].ChangesBeforeInFixesVCS);
            Assert.Equal(1, result[1].ChangesBeforeInFixesBugDB);
            Assert.Equal(2, result[1].ChangesAfter);
            Assert.Equal(1, result[1].ChangesAfterInFixesVCS);
            Assert.Equal(1, result[1].ChangesAfterInFixesBugDB);
            Assert.Equal(2, result[1].NumberOfChanges);
            Assert.Equal(1, result[1].NumberOfChangesWithFixesVCS);
            Assert.Equal(1, result[1].NumberOfChangesWithFixesBugDB);
            Assert.Equal(1, result[1].Authors.Where(a => a.Author == "author1").First().NumberOfChanges);
            Assert.Equal(1, result[1].Authors.Where(a => a.Author == "author2").First().NumberOfChanges);
            Assert.Equal(2, result[1].Authors.Count);
            Assert.Equal(8, result[1].TotalLinesChanged);
            Assert.Equal(4, result[1].TotalLinesChangedInFixesBugDB);
            Assert.Equal(4, result[1].TotalLinesChangedInFixesVCS);
        }

        [Fact]
        public void WhenProcessingFilesShouldExportJsonOutput()
        {
            processor.Process(this.commandLineArgs);
            this.jsonExporter.Verify(m => m.Export(It.IsAny<IList<AggregatedDailyCodeChurn>>(), this.commandLineArgs.OutputFile));
        }
    }
}
