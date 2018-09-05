using CommandLine;
using p4codechurn.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace p4codechurn.unittests
{
    public class GivenACommandLineArgs
    {
        [Fact]
        public void WhenParsingArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "--changes",
                "changes",
                "--describe",
                "describe",
                "--output",
                "output",
                "--output-type",
                "MultipleFile"
            };

            Parser.Default.ParseArguments<CommandLineArgs>(args).WithParsed<CommandLineArgs>((a) =>
            {
                Assert.Equal("changes", a.P4ChangesCommandLine);
                Assert.Equal("describe", a.P4DescribeCommandLine);
                Assert.Equal("output", a.OutputFile);
                Assert.Equal(OutputType.MultipleFile, a.OutputType);
            });
        }
    }
}
