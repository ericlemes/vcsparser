using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class CommandLineArgs
    {
        [Option("changes", HelpText = "p4 changes command line to get changesets. Usually \"p4 changes -s submitted //path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD\" or something similar", Required = true )]
        public string P4ChangesCommandLine { get; set; }

        [Option("describe", HelpText = "p4 describe command line to describe every changeset. Usually \"p4 describe -ds {0}\" should work. {0} will be substituted by the change number during execution", Required = true)]
        public string P4DescribeCommandLine { get; set; }

        [Option("output", HelpText ="File path for single file or file prefix for multiple files.")]
        public string OutputFile { get; set; }

        [Option("output-type", HelpText = "SingleFile or MultipleFile. MultipleFile dumps one file per date.")]
        public OutputType OutputType { get; set; }
    }
}
