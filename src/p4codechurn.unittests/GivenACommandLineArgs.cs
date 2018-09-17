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
        public void WhenParsingExtractArgsShouldReturnExpectedValues()
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

            Parser.Default.ParseArguments<ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (ExtractCommandLineArgs a) => {
                    Assert.Equal("changes", a.P4ChangesCommandLine);
                    Assert.Equal("describe", a.P4DescribeCommandLine);
                    Assert.Equal("output", a.OutputFile);
                    Assert.Equal(OutputType.MultipleFile, a.OutputType);
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => { return 0;  },            
                errs => 1);
        }

        [Fact]
        public void WhenParsingSonarGenericMetricsArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "--inputdir",
                "inputdir",
                "--outputfile",
                "outputfile",
                "--enddate",
                "2018-09-14",
                "--generate1year",
                "false",
                "--generate6months",
                "false",
                "--generate3months",
                "false",
                "--generate30days",
                "false",
                "--generate7days",
                "false",
                "--generate1day",
                "false"
            };

            Parser.Default.ParseArguments<ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (ExtractCommandLineArgs a) =>
                {
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => 
                {
                    Assert.Equal("inputdir", a.InputDir);
                    Assert.Equal("outputfile", a.OutputFile);
                    Assert.Equal(new DateTime(2018, 09, 14), a.EndDate);
                    Assert.False(a.Generate1Year);
                    Assert.False(a.Generate6Months);
                    Assert.False(a.Generate3Months);
                    Assert.False(a.Generate30Days);
                    Assert.False(a.Generate7Days);
                    Assert.False(a.Generate1Day);
                    return 0;            
                },
                errs => 1);
        }

        [Fact]
        public void WhenParsingSonarGenericMetricsArgsAndNoDateShouldNull()
        {
            var args = new List<string>()
            {
                "--inputdir",
                "inputdir",
                "--outputfile",
                "outputfile"
            };

            Parser.Default.ParseArguments<ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (ExtractCommandLineArgs a) =>
                {
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) =>
                {
                    Assert.Null(a.EndDate);                    
                    return 0;
                },
                errs => 1);
        }
    }
}
