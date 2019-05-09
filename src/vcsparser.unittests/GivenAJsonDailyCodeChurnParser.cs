using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Newtonsoft.Json;

namespace vcsparser.unittests
{
    public class GivenAJsonDailyCodeChurnParser
    {
        private JsonListParser<DailyCodeChurn> jsonDailyCodeChurnParser;

        private Mock<IStreamFactory> streamFactory;

        public GivenAJsonDailyCodeChurnParser()
        {
            streamFactory = new Mock<IStreamFactory>();
            this.jsonDailyCodeChurnParser = new JsonListParser<DailyCodeChurn>(streamFactory.Object);
        }

        [Fact]
        public void WhenParsingFileShouldReturnExpectedResults()
        {
            var ms = new MemoryStream();
            this.streamFactory.Setup(m => m.readFile("filename")).Returns(ms);

            var sw = new StreamWriter(ms);
            sw.WriteLine("{\"SchemaVersion\":1,\"Data\":[{\"Timestamp\":\"2018/08/30 00:00:00\",\"FileName\":\"abc\",\"Extension\":\"\",\"Added\":1,\"AddedWithFixes\":0,\"Deleted\":4,\"DeletedWithFixes\":0,\"ChangesBefore\":2,\"ChangesBeforeWithFixes\":0,\"ChangesAfter\":3,\"ChangesAfterWithFixes\":0,\"TotalLinesChanged\":10,\"TotalLinesChangedWithFixes\":0,\"NumberOfChanges\":0,\"NumberOfChangesWithFixes\":0,\"Authors\":[{\"Author\":\"author1\",\"NumberOfChanges\":1},{\"Author\":\"author2\",\"NumberOfChanges\":2}]}]}");
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var result = this.jsonDailyCodeChurnParser.ParseFile("filename");
            var dailyCodeChurn = result.First();
            Assert.Equal("2018/08/30 00:00:00", dailyCodeChurn.Timestamp);
            Assert.Equal("abc", dailyCodeChurn.FileName);            
            Assert.Equal(1, dailyCodeChurn.Added);
            Assert.Equal(4, dailyCodeChurn.Deleted);
            Assert.Equal(2, dailyCodeChurn.ChangesBefore);
            Assert.Equal(3, dailyCodeChurn.ChangesAfter);
            Assert.Equal(10, dailyCodeChurn.TotalLinesChanged);
            Assert.Equal(2, dailyCodeChurn.Authors.Count);
            Assert.Equal("author1", dailyCodeChurn.Authors[0].Author);
            Assert.Equal("author2", dailyCodeChurn.Authors[1].Author);
        }

        [Fact]
        public void WhenParsingFileMissingVersionShouldThrowJsonSerializationException()
        {
            var ms = new MemoryStream();
            this.streamFactory.Setup(m => m.readFile("filename")).Returns(ms);

            var sw = new StreamWriter(ms);
            sw.WriteLine("[{\"Timestamp\":\"2018/08/30 00:00:00\",\"FileName\":\"abc\",\"Extension\":\"\",\"Added\":1,\"AddedWithFixes\":0,\"Deleted\":4,\"DeletedWithFixes\":0,\"ChangesBefore\":2,\"ChangesBeforeWithFixes\":0,\"ChangesAfter\":3,\"ChangesAfterWithFixes\":0,\"TotalLinesChanged\":10,\"TotalLinesChangedWithFixes\":0,\"NumberOfChanges\":0,\"NumberOfChangesWithFixes\":0,\"Authors\":[{\"Author\":\"author1\",\"NumberOfChanges\":1},{\"Author\":\"author2\",\"NumberOfChanges\":2}]}]");
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Action result = () => this.jsonDailyCodeChurnParser.ParseFile("filename");
            Assert.Throws<JsonSerializationException>(result);
        }

        [Fact]
        public void WhenParsingFileWrongVersionShouldThrowException()
        {
            var ms = new MemoryStream();
            this.streamFactory.Setup(m => m.readFile("filename")).Returns(ms);

            var sw = new StreamWriter(ms);
            sw.WriteLine("{\"SchemaVersion\":-1,\"Data\":[{\"Timestamp\":\"2018/08/30 00:00:00\",\"FileName\":\"abc\",\"Extension\":\"\",\"Added\":1,\"AddedWithFixes\":0,\"Deleted\":4,\"DeletedWithFixes\":0,\"ChangesBefore\":2,\"ChangesBeforeWithFixes\":0,\"ChangesAfter\":3,\"ChangesAfterWithFixes\":0,\"TotalLinesChanged\":10,\"TotalLinesChangedWithFixes\":0,\"NumberOfChanges\":0,\"NumberOfChangesWithFixes\":0,\"Authors\":[{\"Author\":\"author1\",\"NumberOfChanges\":1},{\"Author\":\"author2\",\"NumberOfChanges\":2}]}]}");
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Action result = () => this.jsonDailyCodeChurnParser.ParseFile("filename");
            var ex = Assert.Throws<Exception>(result);
            Assert.Equal($"Version mismatch. Expecting {OutputProcessor.SchemaVersion} found {-1}", ex.Message);
        }
    }
}
