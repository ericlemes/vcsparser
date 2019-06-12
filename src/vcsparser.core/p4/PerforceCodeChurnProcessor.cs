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
        private IProcessWrapper processWrapper;
        private IChangesParser changesParser;
        private IDescribeParser describeParser;
        private ICommandLineParser commandLineParser;
        private ILogger logger;
        private IStopWatch stopWatch;
        private IOutputProcessor outputProcessor;
        private IBugDatabaseProcessor bugDatabaseProcessor;
        private IChangesetProcessor changesetProcessor;

        private P4ExtractCommandLineArgs args;

        public PerforceCodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser,IBugDatabaseProcessor bugDatabaseProcessor, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor, P4ExtractCommandLineArgs args)
        {
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.bugDatabaseProcessor = bugDatabaseProcessor;
            this.logger = logger;
            this.stopWatch = stopWatch;
            this.outputProcessor = outputProcessor;
            this.args = args;

            this.changesetProcessor = new ChangesetProcessor(args.BugRegexes, this.logger);
        }

        public void QueryBugDatabase()
        {
            if (string.IsNullOrWhiteSpace(args.BugDatabaseDLL))
                return;
            if (string.IsNullOrWhiteSpace(args.BugDatabaseOutputFile))
                throw new Exception("Dll specified without known output file");

            var bugCache = bugDatabaseProcessor.ProcessBugDatabase(args.BugDatabaseDLL, args.BugDatabaseDllArgs);
            if (bugCache == null)
                return;

            logger.LogToConsole(bugCache.Count + " bug database dates to output");

            this.outputProcessor.ProcessOutput(args.BugDatabaseOutputType, args.BugDatabaseOutputFile, bugCache);
        }

        public int Extract()
        {
            logger.LogToConsole("Invoking: " + args.P4ChangesCommandLine);
            var parsedCommand = this.commandLineParser.ParseCommandLine(args.P4ChangesCommandLine);

            var lines = new List<string>();
            var exitCode = this.processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2, (l) => { lines.Add(l); });
            if (exitCode != 0)
                return exitCode;

            var changes = changesParser.Parse(lines);
            logger.LogToConsole($"Found {changes.Count} changesets to parse");

            this.bugDatabaseProcessor.ProcessCache(args.BugDatabaseOutputFile, this.changesetProcessor);

            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                ReportProgressAfterOneMinute(i, changes);                

                var cmd = commandLineParser.ParseCommandLine(String.Format(args.P4DescribeCommandLine, change));

                var describeLines = new List<string>();
                var describeExitCode = this.processWrapper.Invoke(cmd.Item1, cmd.Item2, (l) => { lines.Add(l); });
                if (describeExitCode != 0)
                    return describeExitCode;

                changesetProcessor.ProcessChangeset(describeParser.Parse(describeLines));

                i++;
            }
            this.stopWatch.Stop();

            this.outputProcessor.ProcessOutput(args.OutputType, args.OutputFile, this.changesetProcessor.Output);
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
