using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public enum OutputType
    {
        SingleFile,
        MultipleFile
    }

    [Verb("p4extract", HelpText = "Extracts code coverage information from p4 and outputs to csv")]
    public class P4ExtractCommandLineArgs
    {
        [Option("changes", HelpText = "p4 changes command line to get changesets. Usually \"p4 changes -s submitted //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD\" or something similar", Required = true )]
        public string P4ChangesCommandLine { get; set; }

        [Option("describe", HelpText = "p4 describe command line to describe every changeset. Usually \"p4 describe -ds {0}\" should work. {0} will be substituted by the change number during execution", Required = true)]
        public string P4DescribeCommandLine { get; set; }

        [Option("output", HelpText ="File path for single file or file prefix for multiple files.", Required = true)]
        public string OutputFile { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = true)]
        public OutputType OutputType { get; set; }
    }

    [Verb("gitextract", HelpText = "Extracts code coverage information from git log file and outputs to csv")]
    public class GitExtractCommandLineArgs
    {
        [Option("gitlogcommand", HelpText = "Command line that will be invoked to get git log. Syntax should be similar to: git log --pretty=fuller --date=iso --after=YYYY-MM-DD --numstat ", Required = true)]
        public string GitLogCommand { get; set; }      

        [Option("output", HelpText = "File path for single file or file prefix for multiple files.", Required = true)]
        public string OutputFile { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date.", Required = true)]
        public OutputType OutputType { get; set; }
    }

    [Verb("sonargenericmetrics", HelpText = "Process csv files and outputs to Sonar Generic Metrics JSON format")]
    public class SonarGenericMetricsCommandLineArgs
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

        [Option("inputdir", HelpText = "Directory with input CSV files", Required = true)]
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
}
