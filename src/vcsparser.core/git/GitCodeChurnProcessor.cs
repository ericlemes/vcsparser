﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core.git
{
    public class GitCodeChurnProcessor
    {
        private ICommandLineParser commandLineParser;

        private IProcessWrapper processWrapper;

        private IGitLogParser gitLogParser;

        private IOutputProcessor outputProcessor;

        private IChangesetProcessor changesetProcessor;

        private IBugDatabaseProcessor bugDatabaseProcessor;

        private ILogger logger;

        private GitExtractCommandLineArgs args;

        public GitCodeChurnProcessor(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser, IOutputProcessor outputProcessor, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, GitExtractCommandLineArgs args)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;
            this.outputProcessor = outputProcessor;
            this.bugDatabaseProcessor = bugDatabaseProcessor;
            this.logger = logger;
            this.args = args;

            this.changesetProcessor = new ChangesetProcessor(this.args.BugRegexes, this.logger);
        }

        public void QueryBugDatabase()
        {
            if (string.IsNullOrWhiteSpace(args.BugDatabaseDLL))
                return;
            if (string.IsNullOrWhiteSpace(args.BugDatabaseOutputFile))
                throw new Exception("Dll specified without known output file");

            var bugCache = bugDatabaseProcessor.ProcessBugDatabase(args.BugDatabaseDLL, args.BugDatabaseDllArgs);

            logger.LogToConsole(bugCache.Count + " bug database dates to output");

            this.outputProcessor.ProcessOutput(bugCache);
        }

        public int Extract()
        {
            logger.LogToConsole("Invoking " + args.GitLogCommand);
            var parsedCommand = this.commandLineParser.ParseCommandLine(args.GitLogCommand);

            var invoke = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
            if (invoke.Item1 != 0)
                return invoke.Item1;

            var changesets = gitLogParser.Parse(invoke.Item2);
            logger.LogToConsole($"Found {changesets.Count} changesets to parse");

            this.bugDatabaseProcessor.ProcessCache(args.BugDatabaseOutputFile, this.changesetProcessor);

            foreach (var changeset in changesets)
                this.changesetProcessor.ProcessChangeset(changeset);

            if (!string.IsNullOrEmpty(this.args.BugRegexes))
                logger.LogToConsole(String.Format("Changesets with bugs: {0}/{1}", this.changesetProcessor.ChangesetsWithBugs, changesets.Count));
            logger.LogToConsole(this.changesetProcessor.Output.Count + " dates to output");

            this.outputProcessor.ProcessOutput(this.changesetProcessor.Output);
            return 0;
        }
    }
}
