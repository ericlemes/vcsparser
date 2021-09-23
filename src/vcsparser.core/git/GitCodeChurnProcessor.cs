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

        private readonly string outputFile;

        private readonly OutputType outputType;

        private readonly string bugDatabaseDLL;

        private readonly string bugDatabaseOutputFile;

        private readonly IEnumerable<string> bugDatabaseDllArgs;

        private readonly string gitLogCommand;

        private readonly string bugRegexes;

        private readonly OutputType bugDatabaseOutputType;

        public GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, GitExtractToCosmosDbCommandLineArgs commandLineArgs) : this(commandLineParser, processWrapper, gitLogParser, outputProcessor, bugDatabaseProcessor, logger, string.Empty, OutputType.CosmosDb, commandLineArgs.BugRegexes, commandLineArgs.BugDatabaseDLL, string.Empty, commandLineArgs.BugDatabaseDllArgs, commandLineArgs.GitLogCommand) { }

        public GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, GitExtractCommandLineArgs commandLineArgs) : this(commandLineParser, processWrapper, gitLogParser, outputProcessor, bugDatabaseProcessor, logger, commandLineArgs.OutputFile, commandLineArgs.OutputType, commandLineArgs.BugRegexes, commandLineArgs.BugDatabaseDLL, commandLineArgs.BugDatabaseOutputFile, commandLineArgs.BugDatabaseDllArgs, commandLineArgs.GitLogCommand)
        {
            if (string.IsNullOrWhiteSpace(bugDatabaseDLL) == false && string.IsNullOrWhiteSpace(bugDatabaseOutputFile))
                throw new Exception("Dll specified without known output file");

            this.bugDatabaseOutputFile = commandLineArgs.BugDatabaseOutputFile;
            this.bugDatabaseOutputType = commandLineArgs.BugDatabaseOutputType;
        }

        private GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, string outputFile, OutputType outputType, string bugRegexes, string bugDatabaseDLL, string bugDatabaseOutputFile, IEnumerable<string> bugDatabaseDllArgs, string gitLogCommand)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;
            this.outputProcessor = outputProcessor;
            this.bugDatabaseProcessor = bugDatabaseProcessor;
            this.logger = logger;
            this.outputType = outputType;
            this.outputFile = outputFile;
            this.bugDatabaseDLL = bugDatabaseDLL;
            this.bugDatabaseOutputFile = bugDatabaseOutputFile;
            this.bugDatabaseDllArgs = bugDatabaseDllArgs;
            this.gitLogCommand = gitLogCommand;
            this.bugRegexes = bugRegexes;

            this.changesetProcessor = new ChangesetProcessor(bugRegexes, this.logger);
        }

        public void QueryBugDatabase()
        {
            if (string.IsNullOrWhiteSpace(bugDatabaseDLL))
                return;

            var bugCache = bugDatabaseProcessor.ProcessBugDatabase(bugDatabaseDLL, bugDatabaseDllArgs);
            
            logger.LogToConsole(bugCache.Count + " bug database dates to output");

            this.outputProcessor.ProcessOutput(bugDatabaseOutputType, bugDatabaseOutputFile, bugCache);
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

            this.outputProcessor.ProcessOutput(outputType, outputFile, this.changesetProcessor.Output);
            return 0;
        }
    }
}
