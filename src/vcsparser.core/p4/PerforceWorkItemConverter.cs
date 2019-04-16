using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core.p4
{
    public class PerforceWorkItemConverter : IWorkItemConverter
    {
        private readonly ICommandLineParser commandLineParser;
        private readonly IProcessWrapper processWrapper;
        private readonly IDescribeParser describeParser;

        private readonly string perforceDescribeCommand;

        public PerforceWorkItemConverter(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IDescribeParser describeParser)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.describeParser = describeParser;

            this.perforceDescribeCommand = "p4 describe -ds {0}";
        }

        private IChangeset Convert(WorkItem item)
        {
            if (!int.TryParse(item.ChangesetId, out _))
                return null;

            var parsedCommandLine = commandLineParser.ParseCommandLine(string.Format(perforceDescribeCommand, item.ChangesetId));
            var stream = processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);
            var changeset = describeParser.Parse(stream);
            return changeset;
        }

        public IDictionary<DateTime, IDictionary<string, IChangeset>> Convert(IDictionary<DateTime, IDictionary<string, WorkItem>> items)
        {
            var dict = new Dictionary<DateTime, IDictionary<string, IChangeset>>();

            foreach (var workItemsByCloseDate in items.Values)
            {
                foreach (var workItem in workItemsByCloseDate.Values)
                {
                    var changeset = Convert(workItem);
                    if (changeset == null)
                        continue;

                    if (!dict.ContainsKey(changeset.ChangesetTimestamp.Date))
                        dict.Add(changeset.ChangesetTimestamp.Date, new Dictionary<string, IChangeset>());

                    dict[changeset.ChangesetTimestamp.Date].Add(changeset.ChangesetIdentifier.ToString(), changeset);
                }
            }
            return dict;
        }
    }
}
