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
    public class GivenACsvParser
    {
        private CsvParser csvParser;

        private Mock<IStreamFactory> streamFactory;

        public GivenACsvParser()
        {
            streamFactory = new Mock<IStreamFactory>();
            this.csvParser = new CsvParser(streamFactory.Object);
        }

        [Fact]
        public void WhenParsingFileShouldReturnExpectedResults()
        {
            var ms = new MemoryStream();
            this.streamFactory.Setup(m => m.readFile("filename")).Returns(ms);

            var sw = new StreamWriter(ms);
            sw.WriteLine("Timestamp,FileName,Extension,Added,Deleted,ChangesBefore,ChangesAfter,TotalLinesChanged");
            sw.WriteLine("2018/11/29 00:00:00,filename.ext,.ext,73,0,22,52,125");
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var result = this.csvParser.ParseFile("filename");
            var dailyCodeChurn = result.First();
            Assert.Equal("2018/11/29 00:00:00", dailyCodeChurn.Timestamp);
            Assert.Equal("filename.ext", dailyCodeChurn.FileName);
            Assert.Equal(".ext", dailyCodeChurn.Extension);
            Assert.Equal(73, dailyCodeChurn.Added);
            Assert.Equal(0, dailyCodeChurn.Deleted);
            Assert.Equal(22, dailyCodeChurn.ChangesBefore);
            Assert.Equal(52, dailyCodeChurn.ChangesAfter);
            Assert.Equal(147, dailyCodeChurn.TotalLinesChanged);
        }
    }
}
