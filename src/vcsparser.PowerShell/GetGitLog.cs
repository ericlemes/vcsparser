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
            var lines = new List<string>();
            var exitCode = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2, WorkingDirectory, (l) => { lines.Add(l); } );
            if (exitCode != 0)
                this.ThrowTerminatingError(new ErrorRecord(new Exception(string.Join(Environment.NewLine, lines)), $"Non zero return code: {exitCode}", ErrorCategory.OperationStopped, null));

            var commits = this.gitLogParser.Parse(lines);
            this.cmdlet.WriteObject(commits);
        }

        public void DoProcessRecord()
        {
            this.ProcessRecord();
        }
    }
}
