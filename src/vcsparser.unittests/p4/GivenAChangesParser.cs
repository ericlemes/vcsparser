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
    public class GivenAChangesParser
    {
        private ChangesParser parser;
        public GivenAChangesParser()
        {
            this.parser = new ChangesParser();
        }

        private List<string> GetListWithContent(string content)
        {
            return new List<string>(content.Split(
                new[] { "\n", Environment.NewLine },
                StringSplitOptions.None
            ));
        }

        [Fact]
        public void WhenParsingChangesWithValidFileShouldReturnChangeNumbers()
        {
            var lines = GetListWithContent(Resources.ChangesFiles1);

            var changeNumbers = this.parser.Parse(lines);

            Assert.Equal(144545, changeNumbers[0]);
            Assert.Equal(144544, changeNumbers[1]);
            Assert.Equal(144541, changeNumbers[2]);
            Assert.Equal(144540, changeNumbers[3]);
            Assert.Equal(144538, changeNumbers[4]);
            Assert.Equal(144537, changeNumbers[5]);
            Assert.Equal(144535, changeNumbers[6]);
            Assert.Equal(144530, changeNumbers[7]);
            Assert.Equal(144527, changeNumbers[8]);
            Assert.Equal(144526, changeNumbers[9]);
        }

        [Fact]
        public void WhenParsingEmptyFileShouldReturnEmptyList()
        {
            var lines = new List<string>();

            var changeNumbers = this.parser.Parse(lines);

            Assert.Empty(changeNumbers);
        }

        [Fact]
        public void WhenParsingFileWithBadContentShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent("someinvalidcontent");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }

        [Fact]
        public void WhenParsingFileWithBadContent2ShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent("some invalid content");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }

        [Fact]
        public void WhenParsingFileWithBadContent3ShouldThrowInvalidFormatException()
        {
            var lines = GetListWithContent("Change shouldbeanumberhere");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(lines); });
        }
    }
}
