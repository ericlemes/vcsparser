using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core.git
{
    public class GitCodeChurnProcessor
    {
        private readonly ICommandLineParser commandLineParser;

        private readonly IProcessWrapper processWrapper;

        private readonly IGitLogParser gitLogParser;

        private readonly IOutputProcessor outputProcessor;

        private readonly IChangesetProcessor changesetProcessor;

        private readonly IBugDatabaseProcessor bugDatabaseProcessor;

        private readonly ILogger logger;

        private readonly string bugDatabaseDLL;

        private readonly string bugDatabaseOutputFile;

        private readonly IEnumerable<string> bugDatabaseDllArgs;

        private readonly string gitLogCommand;

        private readonly string bugRegexes;

        private readonly OutputType outputType;

        public GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, string bugRegexes, string bugDatabaseDLL, string bugDatabaseOutputFile, IEnumerable<string> bugDatabaseDllArgs, string gitLogCommand)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;
            this.outputProcessor = outputProcessor;
            this.bugDatabaseProcessor = bugDatabaseProcessor;
            this.logger = logger;
            this.bugDatabaseDLL = bugDatabaseDLL;
            this.bugDatabaseOutputFile = bugDatabaseOutputFile;
            this.bugDatabaseDllArgs = bugDatabaseDllArgs;
            this.gitLogCommand = gitLogCommand;
            this.bugRegexes = bugRegexes;

            this.changesetProcessor = new ChangesetProcessor(bugRegexes, this.logger);
        }

        public void QueryBugDatabase(bool checkForOutputFile = true)
        {
            if (string.IsNullOrWhiteSpace(bugDatabaseDLL))
                return;
            if (checkForOutputFile && string.IsNullOrWhiteSpace(bugDatabaseOutputFile))
                throw new Exception("Dll specified without known output file");

            var bugCache = bugDatabaseProcessor.ProcessBugDatabase(bugDatabaseDLL, bugDatabaseDllArgs);

            
            logger.LogToConsole(bugCache.Count + " bug database dates to output");

            this.outputProcessor.ProcessOutput(bugCache);
        }

        public int Extract()
        {
            logger.LogToConsole("Invoking " + gitLogCommand);
            var parsedCommand = this.commandLineParser.ParseCommandLine(gitLogCommand);

            var invoke = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
            if (invoke.Item1 != 0)
                return invoke.Item1;

            var changesets = gitLogParser.Parse(invoke.Item2);
            logger.LogToConsole($"Found {changesets.Count} changesets to parse");

            this.bugDatabaseProcessor.ProcessCache(bugDatabaseOutputFile, this.changesetProcessor);

            foreach (var changeset in changesets)
                this.changesetProcessor.ProcessChangeset(changeset);

            if (!string.IsNullOrEmpty(bugRegexes))
                logger.LogToConsole(String.Format("Changesets with bugs: {0}/{1}", this.changesetProcessor.ChangesetsWithBugs, changesets.Count));
            logger.LogToConsole(this.changesetProcessor.Output.Count + " dates to output");

            this.outputProcessor.ProcessOutput(this.changesetProcessor.Output);
            return 0;
        }
    }
}
