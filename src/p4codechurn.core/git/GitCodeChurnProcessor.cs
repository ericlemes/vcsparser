using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core.git
{
    public class GitCodeChurnProcessor
    {
        private ICommandLineParser commandLineParser;

        private IProcessWrapper processWrapper;

        private IGitLogParser gitLogParser;

        private IOutputProcessor outputProcessor;

        private ChangesetProcessor changesetProcessor;

        private ILogger logger;

        public GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, ILogger logger)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;
            this.outputProcessor = outputProcessor;
            this.changesetProcessor = new ChangesetProcessor();
            this.logger = logger;
        }

        public void Extract(GitExtractCommandLineArgs args)
        {
            logger.LogToConsole("Invoking " + args.GitLogCommand);
            var parsedCommand = this.commandLineParser.ParseCommandLine(args.GitLogCommand);            
            var stream = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
            var changesets = gitLogParser.Parse(stream);
            logger.LogToConsole("Found " + changesets.Count + " changesets");
            
            foreach (var changeset in changesets)
                this.changesetProcessor.ProcessChangeset(changeset);
            logger.LogToConsole(this.changesetProcessor.Output.Count + " dates to output");

            this.outputProcessor.ProcessOutput(args.OutputType, args.OutputFile, this.changesetProcessor.Output);           
        }
    }
}
