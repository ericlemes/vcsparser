using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.git;

namespace vcsparser.PowerShell
{
    [Cmdlet(VerbsCommon.Get, "GitLog")]
    public class GetGitLog : Cmdlet, ICmdlet
    {
        private ICommandLineParser commandLineParser;

        private IProcessWrapper processWrapper;

        private IGitLogParser gitLogParser;

        private ICmdlet cmdlet;

        [Parameter(HelpMessage = "Git log command line. For example: git log " +
                     "--pretty=fuller --date=iso --after=YYYY-MM-DD --numstat ", Mandatory = true)]
        public string GitLogCommand
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Working directory to start the process ")]
        public string WorkingDirectory
        {
            get;
            set;
        }

        public GetGitLog()
        {
            this.commandLineParser = new CommandLineParser();
            this.processWrapper = new ProcessWrapper();
            this.gitLogParser = new GitLogParser();
            this.cmdlet = new CmdletAdapter(this);
        }

        public void InjectDependencies(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, 
            IGitLogParser gitLogParser, ICmdlet cmdlet)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;
            this.cmdlet = cmdlet;
        }

        protected override void ProcessRecord()
        {
            var parsedCommand = this.commandLineParser.ParseCommandLine(this.GitLogCommand);
            var invoke = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2, WorkingDirectory);
            if (invoke.Item1 != 0)
                this.ThrowTerminatingError(new ErrorRecord(new Exception(string.Join(Environment.NewLine, invoke.Item2)), $"Non zero return code: {invoke.Item1}", ErrorCategory.OperationStopped, this));

            var commits = this.gitLogParser.Parse(invoke.Item2);
            this.cmdlet.WriteObject(commits);
        }

        public void DoProcessRecord()
        {
            this.ProcessRecord();
        }
    }
}
