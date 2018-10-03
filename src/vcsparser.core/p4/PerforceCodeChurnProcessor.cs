using p4codechurn.core.p4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core.p4
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
        private ChangesetProcessor changesetProcessor;

        public PerforceCodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor)
        {
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.logger = logger;
            this.stopWatch = stopWatch;
            this.outputProcessor = outputProcessor;
            this.changesetProcessor = new ChangesetProcessor();
        }

        private IList<int> ParseChangeSets(string changesCommandLine)
        {
            this.logger.LogToConsole("Invoking: " + changesCommandLine);
            var parsedCommandLine = this.commandLineParser.ParseCommandLine(changesCommandLine);
            var stdOutStream = this.processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);

            return this.changesParser.Parse(stdOutStream);
        }

        public void Extract(OutputType outputType, string outputFileNameOrFilePrefix, string changesCommandLine, string describeCommandLine)
        {
            var changes = ParseChangeSets(changesCommandLine);

            this.logger.LogToConsole(String.Format("Found {0} changesets to parse", changes.Count));            
            
            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                ReportProgressAfterOneMinute(i, changes);                

                var cmd = commandLineParser.ParseCommandLine(String.Format(describeCommandLine, change));
                changesetProcessor.ProcessChangeset(describeParser.Parse(this.processWrapper.Invoke(cmd.Item1, cmd.Item2)));

                i++;
            }
            this.stopWatch.Stop();
                        
            this.outputProcessor.ProcessOutput(outputType, outputFileNameOrFilePrefix, this.changesetProcessor.Output);
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
