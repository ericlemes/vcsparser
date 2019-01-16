using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.p4;

namespace vcsparser.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "PerforceChangeset")]
    public class GetPerforceChangeset : Cmdlet
    {
        private IDescribeParser describeParser;

        private IProcessWrapper processWrapper;

        private ICommandLineParser commandLineParser;

        private ICmdlet cmdlet;

        public GetPerforceChangeset()
        {
            this.processWrapper = new ProcessWrapper();
            this.describeParser = new DescribeParser();
            this.commandLineParser = new CommandLineParser();
            this.cmdlet = new CmdletAdapter(this);
            this.DescribeCommand = "p4 describe -ds {0}";
        }

        public void InjectDependencies(IDescribeParser describeParser, IProcessWrapper processWrapper, ICommandLineParser commandLineParser,
            ICmdlet cmdlet)
        {
            this.processWrapper = processWrapper;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.cmdlet = cmdlet;
        }

        [Parameter(HelpMessage = "Changeset number", Mandatory = true)]
        public int Changeset
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Command to invoke perforce describe. {0} will be substituted by your changeset number")]
        public string DescribeCommand
        {
            get;
            set;
        }

        protected override void ProcessRecord()
        {
            var parsedCommandLine = commandLineParser.ParseCommandLine(String.Format(DescribeCommand, Changeset));
            var stream = processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);
            var changeset = describeParser.Parse(stream);
            this.cmdlet.WriteObject(changeset);
        }        

        public void DoProcessRecord()
        {
            this.ProcessRecord();
        }
    }
}
