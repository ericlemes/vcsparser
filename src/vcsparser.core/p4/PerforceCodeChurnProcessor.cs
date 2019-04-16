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
        private ChangesetProcessor changesetProcessor;

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

        private IList<int> ParseChangeSets(string changesCommandLine)
        {
            this.logger.LogToConsole("Invoking: " + changesCommandLine);
            var parsedCommandLine = this.commandLineParser.ParseCommandLine(changesCommandLine);
            var stdOutStream = this.processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);

            return this.changesParser.Parse(stdOutStream);
        }

        public void Extract()
        {
            var changes = ParseChangeSets(args.P4ChangesCommandLine);

            this.logger.LogToConsole(String.Format("Found {0} changesets to parse", changes.Count));            
            
            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                ReportProgressAfterOneMinute(i, changes);                

                var cmd = commandLineParser.ParseCommandLine(String.Format(args.P4DescribeCommandLine, change));
                changesetProcessor.ProcessChangeset(describeParser.Parse(this.processWrapper.Invoke(cmd.Item1, cmd.Item2)));

                i++;
            }
            this.stopWatch.Stop();

            this.bugDatabaseProcessor.Process(this.changesetProcessor, this.args.DLL, this.args.DllArgs);
                        
            this.outputProcessor.ProcessOutput(args.OutputType, args.OutputFile, this.changesetProcessor.Output);
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
