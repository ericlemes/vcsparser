using vcsparser.core;
using vcsparser.core.p4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenADescribeParser
    {
        private DescribeParser parser;

        public GivenADescribeParser()
        {
            this.parser = new DescribeParser();
        }

        private MemoryStream GetStreamWithContent(string content)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            sw.Write(content);
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        [Fact]
        public void WhenParsingShouldReturnExceptedValues()
        {
            var ms = GetStreamWithContent(Resources.DescribeFile1);

            var result = this.parser.Parse(ms);

            Assert.Equal(141284, result.ChangesetNumber);
            Assert.Equal("author.name@author.name_workspace-machine", result.Author);
            Assert.Equal(new DateTime(2018, 06, 29, 18, 57, 47), result.Timestamp);
            Assert.Equal("Long description of the change set", result.Message);

            Assert.Equal("//depot/Dir1/Dir2/EntityModel/AuctionNotification.cpp", result.FileChanges[0].FileName);
            Assert.Equal(1, result.FileChanges[0].Added);
            Assert.Equal(4, result.FileChanges[0].Deleted);
            Assert.Equal(12, result.FileChanges[0].ChangedBefore);
            Assert.Equal(6, result.FileChanges[0].ChangedAfter);

            Assert.Equal(5, result.FileChanges.Count);
        }

        [Fact]
        public void WhenParsingFileWithInvalidChangeLineShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent(Resources.DescribeFile2);
            
            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }

        [Fact]
        public void WhenParsingFileWithInvalidAuthorShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent(Resources.DescribeFile3);

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }

        [Fact]
        public void WhenParsingFileWithInvalidDateShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent(Resources.DescribeFile4);

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }

        [Fact]
        public void WhenParsingEmptyFileShouldReturnChangesetNumberZero()
        {
            var ms = new MemoryStream();
            var result = this.parser.Parse(ms);
            Assert.Equal(0, result.ChangesetNumber);
        }
    }
}
