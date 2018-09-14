using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public enum OutputType
    {
        SingleFile,
        MultipleFile
    }

    public class CodeChurnProcessor
    {
        private IProcessWrapper processWrapper;
        private IChangesParser changesParser;
        private IDescribeParser describeParser;
        private ICommandLineParser commandLineParser;
        private ILogger logger;
        private IStopWatch stopWatch;
        private IOutputProcessor outputProcessor;

        public CodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser, ILogger logger, IStopWatch stopWatch, IOutputProcessor outputProcessor)
        {
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.logger = logger;
            this.stopWatch = stopWatch;
            this.outputProcessor = outputProcessor;
        }

        private IList<int> ParseChangeSets(string changesCommandLine)
        {
            this.logger.LogToConsole("Invoking: " + changesCommandLine);
            var parsedCommandLine = this.commandLineParser.ParseCommandLine(changesCommandLine);
            var stdOutStream = this.processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);

            return this.changesParser.Parse(stdOutStream);
        }

        public void Process(OutputType outputType, string outputFileNameOrFilePrefix, string changesCommandLine, string describeCommandLine)
        {
            var changes = ParseChangeSets(changesCommandLine);

            this.logger.LogToConsole(String.Format("Found {0} changesets to parse", changes.Count));
            Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>();
            
            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                ReportProgressAfterOneMinute(i, changes);                

                var cmd = commandLineParser.ParseCommandLine(String.Format(describeCommandLine, change));
                ProcessChurn(dict, describeParser.Parse(this.processWrapper.Invoke(cmd.Item1, cmd.Item2)));

                i++;
            }
            this.stopWatch.Stop();

            if (outputType == OutputType.SingleFile)
                this.outputProcessor.ProcessOutputSingleFile(outputFileNameOrFilePrefix, dict);
            else
                this.outputProcessor.ProcessOutputMultipleFile(outputFileNameOrFilePrefix, dict);
        }

        private void ReportProgressAfterOneMinute(int currentChangeset, IList<int> changes)
        {
            if (this.stopWatch.TotalSecondsElapsed() > 60)
            {
                this.logger.LogToConsole(String.Format("Processed {0}/{1} changesets", currentChangeset, changes.Count));
                this.stopWatch.Restart();
            }
        }

        private void ProcessChurn(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict, Changeset changeset)
        {
            if (changeset == null)
                return;

            if (!dict.ContainsKey(changeset.Timestamp.Date))
                dict.Add(changeset.Timestamp.Date, new Dictionary<string, DailyCodeChurn>());

            foreach (var c in changeset.FileChanges)
            {
                if (!dict[changeset.Timestamp.Date].ContainsKey(c.FileName))
                    dict[changeset.Timestamp.Date].Add(c.FileName, new DailyCodeChurn());

                var dailyCodeChurn = dict[changeset.Timestamp.Date][c.FileName];
                dailyCodeChurn.Timestamp = changeset.Timestamp.Date;
                dailyCodeChurn.FileName = c.FileName;
                dailyCodeChurn.Added += c.Added;
                dailyCodeChurn.Deleted += c.Deleted;
                dailyCodeChurn.ChangesBefore += c.ChangedBefore;
                dailyCodeChurn.ChangesAfter += c.ChangedAfter;
            }            
        }
    }
}
