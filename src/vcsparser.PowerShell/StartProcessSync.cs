using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;

namespace vcsparser.PowerShell
{
    [Cmdlet(VerbsLifecycle.Start, "ProcessSync")]
    public class StartProcessSync : Cmdlet, ICmdlet
    {
        private IProcessWrapper processWrapper;
        private ICommandLineParser commandLineParser;
        private ILogger logger;
        private ICmdlet cmdLet;

        public StartProcessSync()
        {
            this.cmdLet = new CmdletAdapter(this);
            this.processWrapper = new ProcessWrapper();
            this.commandLineParser = new CommandLineParser();
            this.logger = new RawConsoleLogger();
        }

        [Parameter(HelpMessage = "Working directory from where the process will be created.", Mandatory = true)]
        public string WorkingDirectory
        {
            get;
            set;
        }

        [Parameter(HelpMessage = "Command line to execute")]
        public string CommandLine
        {
            get;
            set;
        }

        public void InjectDependencies(ICmdlet cmdlet, IProcessWrapper processWrapper,ICommandLineParser commandLineParser, ILogger logger)
        {
            this.cmdLet = cmdlet;
            this.processWrapper = processWrapper;
            this.commandLineParser = commandLineParser;
            this.logger = logger;            
        }

        protected override void ProcessRecord()
        {
            base.ProcessRecord();
            logger.LogToConsole("Executing command line: " + this.CommandLine);

            var cmdLine = this.commandLineParser.ParseCommandLine(this.CommandLine);
            this.cmdLet.WriteObject(this.processWrapper.Invoke(cmdLine.Item1, cmdLine.Item2, this.WorkingDirectory, (l) => { this.logger.LogToConsole(l); }));            
        }

        public void DoProcessRecord()
        {
            this.ProcessRecord();
        }
    }
}
