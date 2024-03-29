﻿using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public enum OutputType
    {
        SingleFile,
        MultipleFile,
        SeparateFiles,
        CosmosDb
    }

    [Verb("p4extract", HelpText = "Extracts code churn information from p4 and outputs to json")]
    public class P4ExtractCommandLineArgs
    {
        [Option("changes", HelpText = "p4 changes command line to get changesets. Usually \"p4 changes -s submitted //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD\" or something similar", Required = true)]
        public string P4ChangesCommandLine { get; set; }

        [Option("describe", HelpText = "p4 describe command line to describe every changeset. Usually \"p4 describe -ds {0}\" should work. {0} will be substituted by the change number during execution", Required = true)]
        public string P4DescribeCommandLine { get; set; }

        [Option("output", HelpText = "File path for single file or file prefix for multiple files.", Required = true)]
        public string OutputFile { get; set; }

        [Option("bugregexes", HelpText = "Regexes, separated by semi colon (;) to identify if this changeset is a bug fix")]
        public string BugRegexes { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = true)]
        public OutputType OutputType { get; set; }

        [Option("bugdatabase-output", HelpText = "BugDatabase: File path for single file or file prefix for multiple files.", Required = false)]
        public string BugDatabaseOutputFile { get; set; }

        [Option("bugdatabase-output-type", HelpText = "BugDatabase: SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = false)]
        public OutputType BugDatabaseOutputType { get; set; }

        [Option("bugdatabase-dll", HelpText = "BugDatabase: File path to the dll to load", Required = false)]
        public string BugDatabaseDLL { get; set; }

        [Option("bugdatabase-args", HelpText = "BugDatabase: Options for the dll", Separator = ' ', Required = false, Min = 1)]
        public IEnumerable<string> BugDatabaseDllArgs { get; set; }
    }

    [Verb("gitextract", HelpText = "Extracts code churn information from git log file and outputs to json")]
    public class GitExtractCommandLineArgs
    {
        [Option("gitlogcommand", HelpText = "Command line that will be invoked to get git log. Syntax should be similar to: git -c core.quotepath=off log --pretty=fuller --date=iso --after=YYYY-MM-DD --numstat ", Required = true)]
        public string GitLogCommand { get; set; }

        [Option("output", HelpText = "File path for single file or file prefix for multiple files.", Required = true)]
        public string OutputFile { get; set; }

        [Option("bugregexes", HelpText = "Regexes, separated by semi colon (;) to identify if this changeset is a bug fix")]
        public string BugRegexes { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = true)]
        public OutputType OutputType { get; set; }

        [Option("bugdatabase-output", HelpText = "BugDatabase: File path for single file or file prefix for multiple files.", Required = false)]
        public string BugDatabaseOutputFile { get; set; }

        [Option("bugdatabase-output-type", HelpText = "BugDatabase: SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = false)]
        public OutputType BugDatabaseOutputType { get; set; }

        [Option("bugdatabase-dll", HelpText = "BugDatabase: File path to the dll to load", Required = false)]
        public string BugDatabaseDLL { get; set; }

        [Option("bugdatabase-args", HelpText = "BugDatabase: Options for the dll", Separator = ' ', Required = false, Min = 1)]
        public IEnumerable<string> BugDatabaseDllArgs { get; set; }
    }

    [Verb("sonargenericmetrics", HelpText = "Process json files in intermediate code churn format and outputs to Sonar Generic Metrics JSON format")]
    public class SonarGenericMetricsCommandLineArgs : ISonarGenericMetrics
    {
        public SonarGenericMetricsCommandLineArgs()
        {
            this.Generate1Day = "";
            this.Generate1Year = "";
            this.Generate30Days = "";
            this.Generate3Months = "";
            this.Generate6Months = "";
            this.Generate7Days = "";
        }

        [Option("fileprefixtoremove", HelpText = "Prefix to remove from file. Usually repository root")]
        public string FilePrefixToRemove { get; set; }

        [Option("inputdir", HelpText = "Directory with input json files", Required = true)]
        public string InputDir { get; set; }

        [Option("outputfile", HelpText = "File to generate json output", Required = true)]
        public string OutputFile { get; set; }

        [Option("enddate", HelpText = "Date to limit the analysis to. ", Required = false)]
        public DateTime? EndDate { get; set; }

        [Option("generate1year", HelpText = "Generates 1 year churn data. ", Default = "true")]
        public string Generate1Year { get; set; }

        [Option("generate6months", HelpText = "Generates 6 months churn data. ", Default = "true")]
        public string Generate6Months { get; set; }

        [Option("generate3months", HelpText = "Generates 3 months churn data. ", Default = "true")]
        public string Generate3Months { get; set; }

        [Option("generate30days", HelpText = "Generates 30 days churn data. ", Default = "true")]
        public string Generate30Days { get; set; }

        [Option("generate7days", HelpText = "Generates 7 days churn data. ", Default = "true")]
        public string Generate7Days { get; set; }

        [Option("generate1day", HelpText = "Generates 1 day churn data. ", Default = "true")]
        public string Generate1Day { get; set; }
    }

    [Verb("dailycodechurn", HelpText = "Process code churn json files in aggregated daily churn report, processing file exclusions")]
    public class DailyCodeChurnCommandLineArgs
    {
        [Option("fileprefixtoremove", HelpText = "Prefix to remove from file. Usually repository root")]
        public string FilePrefixToRemove { get; set; }

        [Option("inputdir", HelpText = "Directory with input json files", Required = true)]
        public string InputDir { get; set; }

        [Option("outputfile", HelpText = "File to generate json output", Required = true)]
        public string OutputFile { get; set; }

        [Option("inclusions", HelpText="SonarQube-stype inclusions expressions", Required = false)]
        public string Inclusions { get; set; }

        [Option("exclusions", HelpText = "SonarQube-style exclusions expressions", Required = false)]
        public string Exclusions { get; set; }
    }

    [Verb("gitextract-to-cosmosdb", HelpText = "Extracts code churn information from git log file and outputs to azure cosmos database")]
    public class GitExtractToCosmosDbCommandLineArgs : ICosmosCommandLineArgs
    {
        [Option("cosmos-db-key", HelpText = "CosmosConnection: Cosmos database key", Required = true)]
        public string CosmosDbKey { get; set; }

        [Option("cosmos-db-database-id", HelpText = "CosmosConnection: Cosmos database id", Required = true)]
        public string DatabaseId { get; set; }

        [Option("cosmos-db-code-churn-cosmos-container", HelpText = "CosmosConnection: Cosmos database container name", Required = true)]
        public string CodeChurnCosmosContainer { get; set; }

        [Option("cosmos-endpoint", HelpText = "CosmosConnection: Cosmos endpoint", Required = true)]
        public string CosmosEndpoint { get; set; }

        [Option("cosmos-project-name", HelpText = "CosmosDocuments: Document's id prefix", Required = true)]
        public string CosmosProjectName { get; set; }

        [Option("gitlogcommand", HelpText = "Command line that will be invoked to get git log. Syntax should be similar to: git -c core.quotepath=off log --pretty=fuller --date=iso --after=YYYY-MM-DD --numstat ", Required = true)]
        public string GitLogCommand { get; set; }

        [Option("bugregexes", HelpText = "Regexes, separated by semi colon (;) to identify if this changeset is a bug fix")]
        public string BugRegexes { get; set; }

        [Option("bugdatabase-dll", HelpText = "BugDatabase: File path to the dll to load", Required = false)]
        public string BugDatabaseDLL { get; set; }

        [Option("bugdatabase-args", HelpText = "BugDatabase: Options for the dll", Separator = ' ', Required = false, Min = 1)]
        public IEnumerable<string> BugDatabaseDllArgs { get; set; }
    }

    [Verb("p4extract-to-cosmosdb", HelpText = "Extracts code churn information from p4 and outputs to cosmos db")]
    public class P4ExtractToCosmosDbCommandLineArgs : ICosmosCommandLineArgs
    {
        [Option("cosmos-db-key", HelpText = "CosmosConnection: Cosmos database key", Required = true)]
        public string CosmosDbKey { get; set; }

        [Option("cosmos-db-database-id", HelpText = "CosmosConnection: Cosmos database id", Required = true)]
        public string DatabaseId { get; set; }

        [Option("cosmos-db-code-churn-cosmos-container", HelpText = "CosmosConnection: Cosmos database container name", Required = true)]
        public string CodeChurnCosmosContainer { get; set; }

        [Option("cosmos-endpoint", HelpText = "CosmosConnection: Cosmos endpoint", Required = true)]
        public string CosmosEndpoint { get; set; }

        [Option("cosmos-project-name", HelpText = "CosmosDocuments: Document's id prefix", Required = true)]
        public string CosmosProjectName { get; set; }

        [Option("changes", HelpText = "p4 changes command line to get changesets. Usually \"p4 changes -s submitted //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD\" or something similar", Required = true)]
        public string P4ChangesCommandLine { get; set; }

        [Option("describe", HelpText = "p4 describe command line to describe every changeset. Usually \"p4 describe -ds {0}\" should work. {0} will be substituted by the change number during execution", Required = true)]
        public string P4DescribeCommandLine { get; set; }

        [Option("bugregexes", HelpText = "Regexes, separated by semi colon (;) to identify if this changeset is a bug fix")]
        public string BugRegexes { get; set; }

        [Option("bugdatabase-dll", HelpText = "BugDatabase: File path to the dll to load", Required = false)]
        public string BugDatabaseDLL { get; set; }

        [Option("bugdatabase-args", HelpText = "BugDatabase: Options for the dll", Separator = ' ', Required = false, Min = 1)]
        public IEnumerable<string> BugDatabaseDllArgs { get; set; }
    }

    [Verb("cosmosdb-download-data", HelpText = "Process code churn documents that are stored in azure cosmos database")]
    public class DownloadFromCosmosDbCommandLineArgs : ICosmosCommandLineArgs
    {
        [Option("cosmos-db-key", HelpText = "CosmosConnection: Cosmos database key", Required = true)]
        public string CosmosDbKey { get; set; }

        [Option("cosmos-db-database-id", HelpText = "CosmosConnection: Cosmos database id", Required = true)]
        public string DatabaseId { get; set; }

        [Option("cosmos-db-code-churn-cosmos-container", HelpText = "CosmosConnection: Cosmos database container name", Required = true)]
        public string CodeChurnCosmosContainer { get; set; }

        [Option("cosmos-endpoint", HelpText = "CosmosConnection: Cosmos endpoint", Required = true)]
        public string CosmosEndpoint { get; set; }

        [Option("output", HelpText = "File path for single file or file prefix for multiple files.", Required = true)]
        public string OutputFile { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date. Separate file will produce one file per change.", Required = true)]
        public OutputType OutputType { get; set; }

        [Option("start-date", HelpText = "Analyzes start date", Required = true)]
        public DateTime? StartDate { get; set; }

        [Option("enddate", HelpText = "Date to limit the analysis to. ", Required = false)]
        public DateTime? EndDate { get; set; }

        [Option("cosmos-document-type", HelpText = "Either CodeChurn or BugDatabase", Required = true)]
        public DocumentType DocumentType { get; set; }

        [Option("cosmos-project-name", HelpText = "CosmosConnection: project's id to query", Required = true)]
        public string CosmosProjectName { get; set; }
    }

    [Verb("sonargenericmetrics-cosmosdb", HelpText = "Process cosmos db documents and produce file in Sonar Generic Metrics JSON format")]
    public class SonarGenericMetricsCosmosDbCommandLineArgs : ICosmosCommandLineArgs, ISonarGenericMetrics
    {
        public SonarGenericMetricsCosmosDbCommandLineArgs()
        {
            this.Generate1Day = "";
            this.Generate1Year = "";
            this.Generate30Days = "";
            this.Generate3Months = "";
            this.Generate6Months = "";
            this.Generate7Days = "";
        }

        [Option("fileprefixtoremove", HelpText = "Prefix to remove from file. Usually repository root")]
        public string FilePrefixToRemove { get; set; }

        [Option("generate1year", HelpText = "Generates 1 year churn data. ", Default = "true")]
        public string Generate1Year { get; set; }

        [Option("generate6months", HelpText = "Generates 6 months churn data. ", Default = "true")]
        public string Generate6Months { get; set; }

        [Option("generate3months", HelpText = "Generates 3 months churn data. ", Default = "true")]
        public string Generate3Months { get; set; }

        [Option("generate30days", HelpText = "Generates 30 days churn data. ", Default = "true")]
        public string Generate30Days { get; set; }

        [Option("generate7days", HelpText = "Generates 7 days churn data. ", Default = "true")]
        public string Generate7Days { get; set; }

        [Option("generate1day", HelpText = "Generates 1 day churn data. ", Default = "true")]
        public string Generate1Day { get; set; }

        [Option("cosmos-db-key", HelpText = "CosmosConnection: Cosmos database key", Required = true)]
        public string CosmosDbKey { get; set; }

        [Option("cosmos-db-database-id", HelpText = "CosmosConnection: Cosmos database id", Required = true)]
        public string DatabaseId { get; set; }

        [Option("cosmos-db-code-churn-cosmos-container", HelpText = "CosmosConnection: Cosmos database container name", Required = true)]
        public string CodeChurnCosmosContainer { get; set; }

        [Option("cosmos-endpoint", HelpText = "CosmosConnection: Cosmos endpoint", Required = true)]
        public string CosmosEndpoint { get; set; }

        [Option("outputfile", HelpText = "File to generate json output", Required = true)]
        public string OutputFile { get; set; }

        [Option("start-date", HelpText = "Analyzes start date", Required = false)]
        public DateTime? StartDate { get; set; }

        [Option("enddate", HelpText = "Date to limit the analysis to. ", Required = false)]
        public DateTime? EndDate { get; set; }

        [Option("cosmos-project-name", HelpText = "CosmosConnection: project's name to query", Required = true)]
        public string CosmosProjectName { get; set; }
    }

    [Verb("data-from-files-to-cosmos-db", HelpText = "Extracts code churn information from json files and outputs to cosmos db")]
    public class DataFromFilesToCosmosDbCommandLineArgs : ICosmosCommandLineArgs
    {
        [Option("path", HelpText = "Folder path for multiple files", Required = true)]
        public string Path { get; set; }

        [Option("cosmos-db-key", HelpText = "CosmosConnection: Cosmos database key", Required = true)]
        public string CosmosDbKey { get; set; }

        [Option("cosmos-endpoint", HelpText = "CosmosConnection: Cosmos endpoint", Required = true)]
        public string CosmosEndpoint { get; set; }

        [Option("cosmos-db-code-churn-cosmos-container", HelpText = "CosmosConnection: Cosmos database container name", Required = true)]
        public string CodeChurnCosmosContainer { get; set; }

        [Option("cosmos-db-database-id", HelpText = "CosmosConnection: Cosmos database id", Required = true)]
        public string DatabaseId { get; set; }

        [Option("cosmos-project-name", HelpText = "CosmosConnection: project's name to query", Required = true)]
        public string CosmosProjectName { get; set; }

        [Option("cosmos-document-type", HelpText = "Either CodeChurn or BugDatabase", Required = true)]
        public DocumentType DocumentType { get; set; }
    }
}
