using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
{
    public class CodeChurnProcessor
    {
        private IProcessWrapper processWrapper;
        private IChangesParser changesParser;
        private IDescribeParser describeParser;
        private ICommandLineParser commandLineParser;
        private ILogger logger;
        private IStopWatch stopWatch;

        public CodeChurnProcessor(IProcessWrapper processWrapper, IChangesParser changesParser, IDescribeParser describeParser, ICommandLineParser commandLineParser, ILogger logger, IStopWatch stopWatch)
        {
            this.processWrapper = processWrapper;
            this.changesParser = changesParser;
            this.describeParser = describeParser;
            this.commandLineParser = commandLineParser;
            this.logger = logger;
            this.stopWatch = stopWatch;
        }

        public IList<DailyCodeChurn> Process(string changesCommandLine, string describeCommandLine)
        {
            this.logger.LogToConsole("Invoking: " + changesCommandLine);
            var parsedCommandLine = this.commandLineParser.ParseCommandLine(changesCommandLine);            
            var stdOutStream = this.processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);

            var changes = this.changesParser.Parse(stdOutStream);
            this.logger.LogToConsole(String.Format("Found {0} changesets to parse", changes.Count));
            Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>();
            
            int i = 0;
            this.stopWatch.Restart();
            
            foreach (var change in changes)
            {
                if (this.stopWatch.TotalSecondsElapsed() > 60)
                {
                    this.logger.LogToConsole(String.Format("Processed {0}/{1} changesets", i, changes.Count));
                    this.stopWatch.Restart();
                }

                var cmd = commandLineParser.ParseCommandLine(String.Format(describeCommandLine, change));
                ProcessChurn(dict, describeParser.Parse(this.processWrapper.Invoke(cmd.Item1, cmd.Item2)));

                i++;
            }
            this.stopWatch.Stop();

            return ConvertDictToOrderedList(dict);
        }

        private IList<DailyCodeChurn> ConvertDictToOrderedList(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            SortedList<DailyCodeChurn, DailyCodeChurn> sortedList = new SortedList<DailyCodeChurn, DailyCodeChurn>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (b.Value.TotalLinesChanged > 0)               
                        sortedList.Add(b.Value, b.Value);                
            return sortedList.Values;
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
