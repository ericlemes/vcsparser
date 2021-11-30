using vcsparser.core.p4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core.p4
{
    public class PerforceCodeChurnProcessor
    {
        private readonly IProcessWrapper processWrapper;
        private readonly IChangesParser changesParser;
        private readonly IDescribeParser describeParser;
        private readonly ICommandLineParser commandLineParser;
        private readonly ILogger logger;
        private readonly IStopWatch stopWatch;
        private readonly IOutputProcessor outputProcessor;
        private readonly IBugDatabaseProcessor bugDatabaseProcessor;
        private readonly IChangesetProcessor changesetProcessor;

        private readonly string outputFile;

        private readonly OutputType outputType;

        private readonly string bugDatabaseDLL;

        private readonly string bugDatabaseOutputFile;

        private readonly IEnumerable<string> bugDatabaseDllArgs;

        private readonly string p4ChangesCommandLine;

        private readonly string p4DescribeCommandLine;

        private readonly OutputType bugDatabaseOutputType;

        public PerforceCodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor, P4ExtractToCosmosDbCommandLineArgs args) : this(processWrapper, changesParser, describeParser, commandLineParser, bugDatabaseProcessor, logger, stopWatch, outputProcessor, string.Empty, OutputType.CosmosDb, args.BugDatabaseDLL, string.Empty, args.BugDatabaseDllArgs, args.P4ChangesCommandLine, args.P4DescribeCommandLine, args.BugRegexes) { }

        public PerforceCodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser, IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor, P4ExtractCommandLineArgs args) : this(processWrapper, changesParser, describeParser, commandLineParser, bugDatabaseProcessor, logger, stopWatch, outputProcessor, args.OutputFile, args.OutputType, args.BugDatabaseDLL, args.BugDatabaseOutputFile, args.BugDatabaseDllArgs, args.P4ChangesCommandLine, args.P4DescribeCommandLine, args.BugRegexes)
        {
            if (string.IsNullOrWhiteSpace(bugDatabaseDLL) == false && string.IsNullOrWhiteSpace(bugDatabaseOutputFile))
                throw new Exception("Dll specified without known output file");

            this.bugDatabaseOutputFile = args.BugDatabaseOutputFile;
            this.bugDatabaseOutputType = args.BugDatabaseOutputType;
        }

        private PerforceCodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser,IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor, string outputFile, OutputType outputType, string bugDatabaseDLL, string bugDatabaseOutputFile, IEnumerable<string> bugDatabaseDllArgs, string p4ChangesCommandLine, string p4DescribeCommandLine, string bugRegexes)
        {
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.bugDatabaseProcessor = bugDatabaseProcessor;
            this.logger = logger;
            this.stopWatch = stopWatch;
            this.outputProcessor = outputProcessor;
            this.outputFile = outputFile;
            this.outputType = outputType;
            this.bugDatabaseDLL = bugDatabaseDLL;
            this.bugDatabaseOutputFile = bugDatabaseOutputFile;
            this.bugDatabaseDllArgs = bugDatabaseDllArgs;
            this.p4ChangesCommandLine = p4ChangesCommandLine;
            this.p4DescribeCommandLine = p4DescribeCommandLine;

            this.changesetProcessor = new ChangesetProcessor(bugRegexes, this.logger);
        }

        public void QueryBugDatabase()
        {
            if (string.IsNullOrWhiteSpace(bugDatabaseDLL))
                return;

            var bugCache = bugDatabaseProcessor.ProcessBugDatabase(bugDatabaseDLL, bugDatabaseDllArgs);
            if (bugCache == null)
                return;

            logger.LogToConsole(bugCache.Count + " bug database dates to output");

            this.outputProcessor.ProcessOutput(bugDatabaseOutputType, bugDatabaseOutputFile, bugCache);
        }

        public int Extract()
        {
            logger.LogToConsole("Invoking: " + p4ChangesCommandLine);
            var parsedCommand = this.commandLineParser.ParseCommandLine(p4ChangesCommandLine);

            var invoke = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
            if (invoke.Item1 != 0)
                return invoke.Item1;

            var changes = changesParser.Parse(invoke.Item2);
            logger.LogToConsole($"Found {changes.Count} changesets to parse");

            this.bugDatabaseProcessor.ProcessCache(this.changesetProcessor);

            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                ReportProgressAfterOneMinute(i, changes);                

                var cmd = commandLineParser.ParseCommandLine(String.Format(p4DescribeCommandLine, change));

                var describeInvoke= this.processWrapper.Invoke(cmd.Item1, cmd.Item2);
                if (describeInvoke.Item1 != 0)
                {
                    this.stopWatch.Stop();
                    return describeInvoke.Item1;
                }

                changesetProcessor.ProcessChangeset(describeParser.Parse(describeInvoke.Item2));

                i++;
            }
            this.stopWatch.Stop();

            this.outputProcessor.ProcessOutput(outputType, outputFile, this.changesetProcessor.Output);
            return 0;
        }

        private void ReportProgressAfterOneMinute(int currentChangeset, IList<int> changes)
        {
            if (this.stopWatch.TotalSecondsElapsed() > 60)
            {
                this.logger.LogToConsole(String.Format("Processed {0}/{1} changesets", currentChangeset, changes.Count));
                this.stopWatch.Restart();
            }
        }
    }
}
