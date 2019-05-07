using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAnOuputProcessor
    {
        private OutputProcessor outputProcessor;
        private Mock<IStreamFactory> streamFactoryMock;
        private Mock<ILogger> loggerMock;
        private MemoryStream outputStream;

        public GivenAnOuputProcessor()
        {
            this.outputStream = new MemoryStream();

            this.streamFactoryMock = new Mock<IStreamFactory>();
            this.streamFactoryMock.Setup(m => m.createFileStream("filename", FileMode.Create, FileAccess.Write)).Returns(outputStream);

            this.loggerMock = new Mock<ILogger>();

            this.outputProcessor = new OutputProcessor(streamFactoryMock.Object, loggerMock.Object);
        }

        [Fact]
        public void WhenProcessingOutputShouldWriteJsonFile()
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

            this.outputProcessor.ProcessOutput(OutputType.SingleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.NotEmpty(resultString);
            Assert.Equal(
                "{\"SchemaVersion\":" + OutputProcessor.SchemaVersion + "," +
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
        public void WhenProcessingOutputAndNoChurnAndSingleFileShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        { "filename", new DailyCodeChurn() { FileName = "abc", Timestamp = "2018/08/30 00:00:00" } }
                    }
                }
            };

            this.outputProcessor.ProcessOutputSingleFile("filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.ToArray());
            Assert.Equal($"{{\"SchemaVersion\":{OutputProcessor.SchemaVersion},\"Data\":[]}}", resultString);
        }

        [Fact]
        public void WhenProcessingOutputAndNoChurnAndMultipleFilesShouldReturnNoOutput()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        { "filename", new DailyCodeChurn() { FileName = "abc", Timestamp = "2018/08/30 00:00:00" } }
                    }
                }
            };

            this.outputProcessor.ProcessOutput(OutputType.MultipleFile, "filename", dict);
            var resultString = UTF8Encoding.UTF8.GetString(this.outputStream.GetBuffer());
            Assert.Empty(resultString);
        }

        [Fact]
        public void WhenProcessingOutputSplitingDateShouldWriteMultipleFiles()
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

            this.outputProcessor.ProcessOutputMultipleFile("filename", dict);

            var resultString1 = UTF8Encoding.UTF8.GetString(output1.ToArray());
            var resultString2 = UTF8Encoding.UTF8.GetString(output2.ToArray());

            Assert.NotEmpty(resultString1);
            Assert.NotEmpty(resultString2);
        }
    }
}
