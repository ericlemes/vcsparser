using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenAChangesParser
    {
        private ChangesParser parser;
        public GivenAChangesParser()
        {
            this.parser = new ChangesParser();
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
        public void WhenParsingChangesWithValidFileShouldReturnChangeNumbers()
        {
            var ms = GetStreamWithContent(Resources.ChangesFiles1);

            var changeNumbers = this.parser.Parse(ms);

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
            var ms = new MemoryStream();

            var changeNumbers = this.parser.Parse(ms);

            Assert.Empty(changeNumbers);
        }

        [Fact]
        public void WhenParsingFileWithBadContentShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent("someinvalidcontent");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }

        [Fact]
        public void WhenParsingFileWithBadContent2ShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent("some invalid content");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }

        [Fact]
        public void WhenParsingFileWithBadContent3ShouldThrowInvalidFormatException()
        {
            var ms = GetStreamWithContent("Change shouldbeanumberhere");

            Assert.Throws<InvalidFormatException>(() => { this.parser.Parse(ms); });
        }
    }
}
