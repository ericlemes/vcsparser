﻿using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using vcsparser.core.bugdatabase;

namespace vcsparser.unittests
{
    public class GivenAJsonOutputProcessor
    {
        private JsonOutputProcessor jsonOutputProcessor;
        private Mock<IStreamFactory> streamFactoryMock;
        private Mock<ILogger> loggerMock;
        private MemoryStream outputStream;

        public GivenAJsonOutputProcessor()
        {
            this.outputStream = new MemoryStream();

            this.streamFactoryMock = new Mock<IStreamFactory>();
            this.streamFactoryMock.Setup(m => m.createFileStream("filename", FileMode.Create, FileAccess.Write)).Returns(outputStream);

            this.loggerMock = new Mock<ILogger>();

            this.jsonOutputProcessor = new JsonOutputProcessor(new DataConverter(), streamFactoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public void WhenDailyCodeChurnProcessingOutputShouldWriteJsonFile()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        {
                            "filename",
                            new DailyCodeChurn()
                            {
                                Added = 1,
                                ChangesBefore = 2,
                                ChangesAfter = 3,
                                Deleted = 4,
                                FileName = "abc",
                                Timestamp = "2018/08/30 00:00:00",
                                Authors = new List<DailyCodeChurnAuthor>() {
                                    new DailyCodeChurnAuthor()
                                    {
                                        Author = "author1",
                                        NumberOfChanges = 1
                                    },
                                    new DailyCodeChurnAuthor()
                                    {
                                        Author = "author2",
                                        NumberOfChanges = 2
                                    }
                                }
                            }
                        }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutput(OutputType.SingleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.NotEmpty(resultString);
            Assert.Equal(
                "{\"SchemaVersion\":" + JsonOutputData<DailyCodeChurn>.CurrentVersion + "," +
                "\"Data\":[{" +
                "\"Timestamp\":\"2018/08/30 00:00:00\"," +
                "\"FileName\":\"abc\"," +
                "\"Extension\":\"\"," +
                "\"Added\":1," +
                "\"AddedWithFixes\":0," +
                "\"Deleted\":4," +
                "\"DeletedWithFixes\":0," +
                "\"ChangesBefore\":2," +
                "\"ChangesBeforeWithFixes\":0," +
                "\"ChangesAfter\":3," +
                "\"ChangesAfterWithFixes\":0," +
                "\"TotalLinesChanged\":10," +
                "\"TotalLinesChangedWithFixes\":0," +
                "\"NumberOfChanges\":0," +
                "\"NumberOfChangesWithFixes\":0," +
                "\"Authors\":[{\"Author\":\"author1\",\"NumberOfChanges\":1},{\"Author\":\"author2\",\"NumberOfChanges\":2}]}]}", resultString);
        }

        [Fact]
        public void WhenDailyCodeChurnProcessingOutputAndNoChurnAndSingleFileShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        { "filename", new DailyCodeChurn() { FileName = "abc", Timestamp = "2018/08/30 00:00:00" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutputSingleFile("filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.Equal($"{{\"SchemaVersion\":{JsonOutputData<DailyCodeChurn>.CurrentVersion},\"Data\":[]}}", resultString);
        }

        [Fact]
        public void WhenDailyCodeChurnProcessingOutputAndNoChurnAndMultipleFilesShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        { "filename", new DailyCodeChurn() { FileName = "abc", Timestamp = "2018/08/30 00:00:00" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutput(OutputType.MultipleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.GetBuffer());
            Assert.Empty(resultString);
        }

        [Fact]
        public void WhenDailyCodeChurnProcessingOutputSplitingDateShouldWriteMultipleFiles()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>();
            dict.Add(new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>());
            dict[new DateTime(2018, 08, 30)].Add("abc", new DailyCodeChurn()
            {
                Added = 1,
                ChangesBefore = 2,
                ChangesAfter = 3,
                Deleted = 4,
                FileName = "abc",
                Timestamp = "2018/08/30 00:00:00"
            });
            dict.Add(new DateTime(2018, 08, 31), new Dictionary<string, DailyCodeChurn>());
            dict[new DateTime(2018, 08, 31)].Add("abc", new DailyCodeChurn()
            {
                Added = 1,
                ChangesBefore = 2,
                ChangesAfter = 3,
                Deleted = 4,
                FileName = "abc",
                Timestamp = "2018/08/31 00:00:00"
            });

            var output1 = new MemoryStream();
            var output2 = new MemoryStream();

            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-30.json", FileMode.Create, FileAccess.Write)).Returns(output1);
            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-31.json", FileMode.Create, FileAccess.Write)).Returns(output2);

            this.jsonOutputProcessor.ProcessOutputMultipleFile("filename", dict);

            var resultString1 = UTF8Encoding.UTF8.GetString(output1.ToArray());
            var resultString2 = UTF8Encoding.UTF8.GetString(output2.ToArray());

            Assert.NotEmpty(resultString1);
            Assert.NotEmpty(resultString2);
        }

        [Fact]
        public void WhenWorkItemProcessingOutputShouldWriteJsonFile()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>()
                    {
                        {
                            "Some Change Set Id",
                            new WorkItem()
                            {
                                ChangesetId = "Some Change Set Id",
                                ClosedDate = new DateTime(2018, 08, 30),
                                WorkItemId = "Some Work Item Id"
                            }
                        }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutput(OutputType.SingleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.NotEmpty(resultString);
            Assert.Equal(
                "{\"SchemaVersion\":" + JsonOutputData<DailyCodeChurn>.CurrentVersion + "," +
                "\"Data\":[{" +
                "\"ClosedDate\":\"2018/08/30 00:00:00\"," +
                "\"WorkItemId\":\"Some Work Item Id\"," +
                "\"ChangesetId\":\"Some Change Set Id\"" +
                "}]}", resultString);
        }

        [Fact]
        public void WhenWorkitemProcessingOutputAnChangeSetAndSingleFileShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>()
                    {
                        { "filename", new WorkItem() { ClosedDate = new DateTime(2018, 08, 30), WorkItemId = "Some Work Item Id" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutputSingleFile("filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.Equal(
                "{\"SchemaVersion\":" + JsonOutputData<DailyCodeChurn>.CurrentVersion + "," +
                "\"Data\":[]}", resultString);
        }

        [Fact]
        public void WhenWorkItemProcessingOutputAndNoChangeSetAndMultipleFilesShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>()
                    {
                        { "filename", new WorkItem() { ClosedDate = new DateTime(2018, 08, 30), WorkItemId = "Some Work Item Id" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutput(OutputType.MultipleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.GetBuffer());
            Assert.Empty(resultString);
        }

        [Fact]
        public void WhenWorkItemProcessingOutputSplitingDateShouldWriteMultipleFiles()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            dict.Add(new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 30)].Add("Some Change Set Id 1", new WorkItem()
            {
                ChangesetId = "Some Change Set Id 1",
                ClosedDate = new DateTime(2018, 08, 30),
                WorkItemId = "Some Work Item Id 1"
            });
            dict.Add(new DateTime(2018, 08, 31), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 31)].Add("Some Change Set Id 2", new WorkItem()
            {
                ChangesetId = "Some Change Set Id 2",
                ClosedDate = new DateTime(2018, 08, 31),
                WorkItemId = "Some Work Item Id 2"
            });

            var output1 = new MemoryStream();
            var output2 = new MemoryStream();

            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-30.json", FileMode.Create, FileAccess.Write)).Returns(output1);
            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-31.json", FileMode.Create, FileAccess.Write)).Returns(output2);

            this.jsonOutputProcessor.ProcessOutputMultipleFile("filename", dict);

            var resultString1 = UTF8Encoding.UTF8.GetString(output1.ToArray());
            var resultString2 = UTF8Encoding.UTF8.GetString(output2.ToArray());

            Assert.NotEmpty(resultString1);
            Assert.NotEmpty(resultString2);
        }

        [Fact]
        public void WhenConvertToOrderedListUnknownShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, SomeUnimplementedClass>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, SomeUnimplementedClass>()
                    {
                        { "filename", new SomeUnimplementedClass() { SomeData = "Some Data" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutputSingleFile("filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.Equal(
                "{\"SchemaVersion\":" + JsonOutputData<SomeUnimplementedClass>.CurrentVersion + "," +
                "\"Data\":[]}", resultString);
        }

        [Fact]
        public void WhenConvertToOrderedListPerDayUnknownShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, SomeUnimplementedClass>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, SomeUnimplementedClass>()
                    {
                        { "filename", new SomeUnimplementedClass() { SomeData = "Some Data" } }
                    }
                }
            };

            this.jsonOutputProcessor.ProcessOutputMultipleFile("filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.GetBuffer());
            Assert.Empty(resultString);
        }

        [Fact]
        public void WhenConvertToOrderedListAsSeparateFilesShouldReturnExpectedOutput()
        {
            this.jsonOutputProcessor = new JsonOutputProcessor(new DataConverter(), streamFactoryMock.Object, loggerMock.Object);

            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            dict.Add(new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 30)].Add("Some Change Set Id 1", new WorkItem()
            {
                ChangesetId = "Some Change Set Id 1",
                ClosedDate = new DateTime(2018, 08, 30),
                WorkItemId = "Some Work Item Id 1"
            });
            dict.Add(new DateTime(2018, 08, 31), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 31)].Add("Some Change Set Id 2", new WorkItem()
            {
                ChangesetId = "Some Change Set Id 2",
                ClosedDate = new DateTime(2018, 08, 31),
                WorkItemId = "Some Work Item Id 2"
            });

            var output1 = new MemoryStream();
            var output2 = new MemoryStream();

            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-30_Some Work Item Id 1_Some Change Set Id 1.json", FileMode.Create, FileAccess.Write)).Returns(output1);
            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-31_Some Work Item Id 2_Some Change Set Id 2.json", FileMode.Create, FileAccess.Write)).Returns(output2);

            this.jsonOutputProcessor.ProcessOutputSeparateFiles("filename", dict);

            var resultString1 = Encoding.UTF8.GetString(output1.ToArray());
            var resultString2 = Encoding.UTF8.GetString(output2.ToArray());

            Assert.NotEmpty(resultString1);
            Assert.NotEmpty(resultString2);
        }

        [Fact]
        public void WhenProcessingSeparateFilesOutputShouldWriteJsonFile()
        {
            this.jsonOutputProcessor = new JsonOutputProcessor(new DataConverter(), streamFactoryMock.Object, loggerMock.Object);

            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            dict.Add(new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 30)].Add("Some Change Set Id", new WorkItem()
            {
                ChangesetId = "Some Change Set Id",
                ClosedDate = new DateTime(2018, 08, 30),
                WorkItemId = "Some Work Item Id"
            });

            var output = new MemoryStream();

            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-30_Some Work Item Id_Some Change Set Id.json", FileMode.Create, FileAccess.Write)).Returns(output);

            this.jsonOutputProcessor.ProcessOutput(OutputType.SeparateFiles, "filename", dict);
            var resultString = Encoding.UTF8.GetString(output.ToArray());
            Assert.NotEmpty(resultString);
            Assert.Equal(
                "{\"SchemaVersion\":" + JsonOutputData<DailyCodeChurn>.CurrentVersion + "," +
                "\"Data\":[{" +
                "\"ClosedDate\":\"2018/08/30 00:00:00\"," +
                "\"WorkItemId\":\"Some Work Item Id\"," +
                "\"ChangesetId\":\"Some Change Set Id\"" +
                "}]}", resultString);
        }

        [Fact]
        public void WhenProcessingOutputTypeCosmosDbShouldDoNothing()
        {
            this.jsonOutputProcessor = new JsonOutputProcessor(new DataConverter(), streamFactoryMock.Object, loggerMock.Object);

            var dict = new Dictionary<DateTime, Dictionary<string, WorkItem>>();
            dict.Add(new DateTime(2018, 08, 30), new Dictionary<string, WorkItem>());
            dict[new DateTime(2018, 08, 30)].Add("Some Change Set Id", new WorkItem()
            {
                ChangesetId = "Some Change Set Id",
                ClosedDate = new DateTime(2018, 08, 30),
                WorkItemId = "Some Work Item Id"
            });

            var output = new MemoryStream();

            this.streamFactoryMock.Setup(m => m.createFileStream("filename_2018-08-30_Some Work Item Id_Some Change Set Id.json", FileMode.Create, FileAccess.Write)).Returns(output);

            this.jsonOutputProcessor.ProcessOutput(OutputType.CosmosDb, "filename", dict);
            var resultString = Encoding.UTF8.GetString(output.ToArray());

            Assert.Empty(resultString);
        }
    }

    class SomeUnimplementedClass : IOutputJson
    {
        public string SomeData { get; set; }

        public string GetFileLongName()
        {
            return SomeData;
        }
    }
}