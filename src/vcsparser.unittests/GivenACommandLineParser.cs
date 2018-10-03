using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenACommandLineParser
    {
        [Fact]
        public void WhenParsingSimpleCommandLineShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("simpleexecutable.exe arg1 arg2 arg3");
            Assert.Equal("simpleexecutable.exe", cmd.Item1);
            Assert.Equal("arg1 arg2 arg3", cmd.Item2);
        }

        [Fact]
        public void WhenParsingOpenQuoteWithoutClosingShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("\"open quote without closing");
            Assert.Equal("\"open quote without closing", cmd.Item1);
            Assert.Equal("", cmd.Item2);
        }

        [Fact]
        public void WhenParsingLongNameExecutableShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("\"c:\\program file\\complex path\\app.exe\" arg arg arg");
            Assert.Equal("c:\\program file\\complex path\\app.exe", cmd.Item1);
            Assert.Equal("arg arg arg", cmd.Item2);
        }


        [Fact]
        public void WhenParsingLongNameExecutableWithNoArgsShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("\"c:\\program file\\complex path\\app.exe\"");
            Assert.Equal("c:\\program file\\complex path\\app.exe", cmd.Item1);
            Assert.Equal("", cmd.Item2);
        }

        [Fact]
        public void WhenParsingSingleQuoteShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("\"");
            Assert.Equal("", cmd.Item1);
            Assert.Equal("", cmd.Item2);
        }

        [Fact]
        public void WhenParsingDoubleQuoteShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("\"\"");
            Assert.Equal("", cmd.Item1);
            Assert.Equal("", cmd.Item2);
        }

        [Fact]
        public void WhenParsingSmallStringShouldReturnExpectedValue()
        {
            var parser = new CommandLineParser();
            var cmd = parser.ParseCommandLine("a");
            Assert.Equal("a", cmd.Item1);
            Assert.Equal("", cmd.Item2);
        }
    }
}
