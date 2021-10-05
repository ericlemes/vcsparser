using CommandLine;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.Database.Cosmos;
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
                "exclusions",
                "--inclusions",
                "inclusions"
            };

            Parser.Default.ParseArguments<DailyCodeChurnCommandLineArgs>(args)
                .MapResult(
                (DailyCodeChurnCommandLineArgs a) =>
                {
                    Assert.Equal("prefix", a.FilePrefixToRemove);
                    Assert.Equal("inputdir", a.InputDir);
                    Assert.Equal("outputfile", a.OutputFile);
                    Assert.Equal("exclusions", a.Exclusions);
                    Assert.Equal("inclusions", a.Inclusions);

                    return 0;
                },
                
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }

        [Fact]
        public void WhenParsingGitExtractToCosmosDbCommandLineArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "gitextract-to-cosmosdb",
                "--cosmos-db-key",
                "cosmos-db-key",
                "--cosmos-db-database-id",
                "cosmos-db-database-id",
                "--cosmos-db-code-churn-cosmos-container",
                "cosmos-db-code-churn-cosmos-container",
                "--cosmos-endpoint",
                "cosmos-endpoint",
                "--cosmos-project-name",
                "cosmos-project-name",
                "--gitlogcommand",
                "gitlogcommand",
                "--bugregexes",
                "bugregexes",
                "--bugdatabase-dll",
                "bugdatabase-dll",
                "--bugdatabase-args",
                "bugdatabase-args1 bugdatabase-args2",
            };

            Parser.Default.ParseArguments<GitExtractToCosmosDbCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                    (GitExtractToCosmosDbCommandLineArgs a) => {
                        Assert.Equal("cosmos-db-key", a.CosmosDbKey);
                        Assert.Equal("cosmos-db-database-id", a.DatabaseId);
                        Assert.Equal("cosmos-db-code-churn-cosmos-container", a.CodeChurnCosmosContainer);
                        Assert.Equal("cosmos-endpoint", a.CosmosEndpoint);
                        Assert.Equal("cosmos-project-name", a.CosmosProjectName);
                        Assert.Equal("gitlogcommand", a.GitLogCommand);
                        Assert.Equal("bugregexes", a.BugRegexes);
                        Assert.Equal("bugdatabase-dll", a.BugDatabaseDLL);

                        var bugDatabaseDllArgsList = a.BugDatabaseDllArgs.ToList();
                        Assert.Equal("bugdatabase-args1", bugDatabaseDllArgsList[0]);
                        Assert.Equal("bugdatabase-args2", bugDatabaseDllArgsList[1]);
                        return 0;
                    },
                    (SonarGenericMetricsCommandLineArgs a) =>
                    {
                        Assert.Null(a.EndDate);
                        return 0;
                    },
                    (IEnumerable<Error> errs) => {
                        throw new Exception("Should not fail.");
                    } );
        }

        [Fact]
        public void WhenParsingDownloadFromCosmosDbCommandLineArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "cosmosdb-download-data",
                "--cosmos-db-key",
                "cosmos-db-key",
                "--cosmos-db-database-id",
                "cosmos-db-database-id",
                "--cosmos-db-code-churn-cosmos-container",
                "cosmos-db-code-churn-cosmos-container",
                "--cosmos-endpoint",
                "cosmos-endpoint",
                "--output",
                "output",
                "--output-type",
                "SeparateFiles",
                "--start-date",
                "2018-09-14",
                "--enddate",
                "2021-09-14",
                "--cosmos-document-type",
                "CodeChurn",
                "--cosmos-project-name",
                "some-project-name"
            };

            Parser.Default.ParseArguments<DownloadFromCosmosDbCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                    (DownloadFromCosmosDbCommandLineArgs a) => {
                        Assert.Equal("cosmos-db-key", a.CosmosDbKey);
                        Assert.Equal("cosmos-db-database-id", a.DatabaseId);
                        Assert.Equal("cosmos-db-code-churn-cosmos-container", a.CodeChurnCosmosContainer);
                        Assert.Equal("cosmos-endpoint", a.CosmosEndpoint);
                        Assert.Equal("output", a.OutputFile);
                        Assert.Equal("some-project-name", a.CosmosProjectName);
                        Assert.Equal(OutputType.SeparateFiles, a.OutputType);
                        Assert.Equal(new DateTime(2018, 09, 14), a.StartDate);
                        Assert.Equal(new DateTime(2021, 09, 14), a.EndDate);
                        Assert.Equal(DocumentType.CodeChurn, a.DocumentType);

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
        public void WhenParsingSonarGenericMetricsCosmosDbCommandLineArgsShouldReturnExpectedValues()
        {
            var args = new List<string>()
            {
                "sonargenericmetrics-cosmosdb",
                "--fileprefixtoremove",
                "prefix",
                "--outputfile",
                "outputfile",
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
                "false",
                "--cosmos-db-key",
                "cosmos-db-key",
                "--cosmos-db-database-id",
                "cosmos-db-database-id",
                "--cosmos-db-code-churn-cosmos-container",
                "cosmos-db-code-churn-cosmos-container",
                "--cosmos-endpoint",
                "cosmos-endpoint",
                "--start-date",
                "2018-09-14",
                "--enddate",
                "2021-09-14",
                "--cosmos-project-name",
                "some-project-name"
            };

            Parser.Default.ParseArguments<SonarGenericMetricsCosmosDbCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                (SonarGenericMetricsCosmosDbCommandLineArgs a) =>
                {
                    return 0;
                },
                (SonarGenericMetricsCosmosDbCommandLineArgs a) =>
                {
                    Assert.Equal("prefix", a.FilePrefixToRemove);
                    Assert.Equal("outputfile", a.OutputFile);
                    Assert.Equal(new DateTime(2018, 09, 14), a.EndDate);
                    Assert.Equal(new DateTime(2018, 09, 14), a.StartDate);
                    Assert.False(a.Generate1Year.ToLower() == "true");
                    Assert.False(a.Generate6Months.ToLower() == "true");
                    Assert.False(a.Generate3Months.ToLower() == "true");
                    Assert.False(a.Generate30Days.ToLower() == "true");
                    Assert.False(a.Generate7Days.ToLower() == "true");
                    Assert.False(a.Generate1Day.ToLower() == "true");
                    Assert.Equal("cosmos-db-key", a.CosmosDbKey);
                    Assert.Equal("cosmos-db-database-id", a.DatabaseId);
                    Assert.Equal("cosmos-db-code-churn-cosmos-container", a.CodeChurnCosmosContainer);
                    Assert.Equal("cosmos-endpoint", a.CosmosEndpoint);
                    Assert.Equal("some-project-name", a.CosmosProjectName);
                    return 0;
                },
                (IEnumerable<Error> errs) => {
                    throw new Exception("Should not fail.");
                });
        }
    }
}
