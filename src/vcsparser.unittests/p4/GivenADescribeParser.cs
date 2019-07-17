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

        private List<string> GetListWithContent(string content)
        {
            return new List<string>(content.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            ));
        }

        [Fact]
        public void WhenParsingShouldReturnExceptedValues()
        {
            var lines = GetListWithContent(Resources.DescribeFile1);

            var result = this.parser.Parse(lines);

            Assert.Equal(141284, result.ChangesetNumber);
            Assert.Equal("author.name@author.name_workspace-machine", result.Author);
            Assert.Equal(new DateTime(2018, 06, 29, 18, 57, 47), result.ChangesetTimestamp);
            Assert.Equal("Long description of the change set" + Environment.NewLine + "with multiple lines" + Environment.NewLine, result.ChangesetMessage);

            Assert.Equal("//depot/Dir1/Dir2/EntityModel/AuctionNotification.cpp", result.ChangesetFileChanges[0].FileName);
            Assert.Equal(1, result.ChangesetFileChanges[0].Added);
            Assert.Equal(4, result.ChangesetFileChanges[0].Deleted);
            Assert.Equal(12, result.ChangesetFileChanges[0].ChangedBefore);
            Assert.Equal(6, result.ChangesetFileChanges[0].ChangedAfter);

            Assert.Equal(5, result.ChangesetFileChanges.Count);
        }

        [Fact]
        public void WhenParsingFileWithInvalidChangeLineShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent(Resources.DescribeFile2);
            
            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }

        [Fact]
        public void WhenParsingFileWithInvalidAuthorShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent(Resources.DescribeFile3);

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }

        [Fact]
        public void WhenParsingFileWithInvalidDateShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent(Resources.DescribeFile4);

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }

        [Fact]
        public void WhenParsingEmptyFileShouldReturnChangesetNumberZero()
        {
            var lines = new List<string>();
            var result = this.parser.Parse(lines);
            Assert.Equal(0, result.ChangesetNumber);
        }
    }
}
