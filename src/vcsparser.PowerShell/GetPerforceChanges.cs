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
    [Cmdlet(VerbsCommon.Get, "PerforceChanges")]
    public class GetPerforceChanges : Cmdlet
    {
        private ICommandLineParser commandLineParser;

        private IProcessWrapper processWrapper;

        private IChangesParser changesParser;

        private ICmdlet cmdlet;

        public GetPerforceChanges()
        {
            this.commandLineParser = new CommandLineParser();
            this.processWrapper = new ProcessWrapper();
            this.changesParser = new ChangesParser();
            this.cmdlet = new CmdletAdapter(this);
        }

        [Parameter(HelpMessage = "Perforce command line to invoke p4 changes. For example: p4 changes -s submitted " +
                   "//path/to/your/depot/...@YYYY/MM/DD,YYYY/MM/DD", Mandatory = true)]
        public string ChangesCommand {
            get;
            set;
        }

        public void InjectDependencies(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, 
            IChangesParser changesParser, ICmdlet cmdlet)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.cmdlet = cmdlet;
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();

            var commandLine = this.commandLineParser.ParseCommandLine(this.ChangesCommand);
            var invoke = this.processWrapper.Invoke(commandLine.Item1, commandLine.Item2);
            if (invoke.Item1 != 0)
                this.ThrowTerminatingError(new ErrorRecord(new Exception(string.Join(Environment.NewLine, invoke.Item2)), $"Non zero return code: {invoke.Item1}", ErrorCategory.OperationStopped, this));

            var changes = this.changesParser.Parse(invoke.Item2);
            this.cmdlet.WriteObject(changes);
        }

        public void DoProcessRecord()
        {
            this.ProcessRecord();            
        }
    }
}
