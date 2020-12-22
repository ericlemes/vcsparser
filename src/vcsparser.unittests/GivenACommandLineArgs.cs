using CommandLine;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenACommandLineArgs
    {
        [Fact]
        public void WhenParsingp4ExtractArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "p4extract",
                "--changes",
                "changes",
                "--describe",
                "describe",
                "--output",
                "output",
                "--output-type",
                "MultipleFile"
            };

            Parser.Default.ParseArguments<P4ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (P4ExtractCommandLineArgs a) => {
                    Assert.Equal("changes", a.P4ChangesCommandLine);
                    Assert.Equal("describe", a.P4DescribeCommandLine);
                    Assert.Equal("output", a.OutputFile);
                    Assert.Equal(OutputType.MultipleFile, a.OutputType);                    
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => { return 0;  },            
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                } );
        }

        [Fact]
        public void WhenParsingp4ExtractWithBugRegexesArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "p4extract",
                "--changes",
                "changes",
                "--describe",
                "describe",
                "--output",
                "output",
                "--output-type",
                "MultipleFile",
                "--bugregexes",
                "regex1; regex2; regex3"
            };

            Parser.Default.ParseArguments<P4ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (P4ExtractCommandLineArgs a) => {
                    Assert.Equal("changes", a.P4ChangesCommandLine);
                    Assert.Equal("describe", a.P4DescribeCommandLine);
                    Assert.Equal("output", a.OutputFile);
                    Assert.Equal(OutputType.MultipleFile, a.OutputType);
                    Assert.Equal("regex1; regex2; regex3", a.BugRegexes);
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => { return 0; },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingGitExtractArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "gitextract",
                "--gitlogcommand",
                "gitlogcommand",
                "--output",
                "output",
                "--output-type",
                "MultipleFile"
            };

            Parser.Default.ParseArguments<GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (GitExtractCommandLineArgs a) => {
                    Assert.Equal("gitlogcommand", a.GitLogCommand);                    
                    Assert.Equal("output", a.OutputFile);
                    Assert.Equal(OutputType.MultipleFile, a.OutputType);
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => { return 0; },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingGitExtractArgsWithBugOptionsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "gitextract",
                "--gitlogcommand",
                "gitlogcommand",
                "--output",
                "output",
                "--output-type",
                "MultipleFile",
                "--bugregexes",
                "regex1; regex2; regex3"
            };

            Parser.Default.ParseArguments<GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (GitExtractCommandLineArgs a) => {
                    Assert.Equal("regex1; regex2; regex3", a.BugRegexes);                    
                    Assert.Equal(OutputType.MultipleFile, a.OutputType);
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => { return 0; },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingSonarGenericMetricsArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "sonargenericmetrics",
                "--fileprefixtoremove",
                "prefix",
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

            Parser.Default.ParseArguments<P4ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (P4ExtractCommandLineArgs a) =>
                {
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) => 
                {
                    Assert.Equal("prefix", a.FilePrefixToRemove);
                    Assert.Equal("inputdir", a.InputDir);
                    Assert.Equal("outputfile", a.OutputFile);
                    Assert.Equal(new DateTime(2018, 09, 14), a.EndDate);
                    Assert.False(a.Generate1Year.ToLower() == "true");
                    Assert.False(a.Generate6Months.ToLower() == "true");
                    Assert.False(a.Generate3Months.ToLower() == "true");
                    Assert.False(a.Generate30Days.ToLower() == "true");
                    Assert.False(a.Generate7Days.ToLower() == "true");
                    Assert.False(a.Generate1Day.ToLower() == "true");
                    return 0;            
                },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingSonarGenericMetricsArgsAndNoDateShouldNull()
        {
            var args = new List<string>()
            {
                "sonargenericmetrics",
                "--fileprefixtoremove",
                "prefix",
                "--inputdir",
                "inputdir",
                "--outputfile",
                "outputfile"
            };

            Parser.Default.ParseArguments<P4ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (P4ExtractCommandLineArgs a) =>
                {
                    return 0;
                },
                (SonarGenericMetricsCommandLineArgs a) =>
                {
                    Assert.Null(a.EndDate);                    
                    return 0;
                },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingDailyChurnReportShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "dailychurnreport",
                "--fileprefixtoremove",
                "prefix",
                "--inputdir",
                "inputdir",
                "--outputfile",
                "outputfile",
                "--exclusions",
                "exclusions"
            };

            Parser.Default.ParseArguments<DailyCodeChurnCommandLineArgs>(args)
                .MapResult(
                (DailyCodeChurnCommandLineArgs a) =>
                {
                    Assert.Equal("prefix", a.FilePrefixToRemove);
                    Assert.Equal("inputdir", a.InputDir);
                    Assert.Equal("outputfile", a.OutputFile);
                    Assert.Equal("exclusions", a.Exclusions);

                    return 0;
                },
                
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }
    }
}
