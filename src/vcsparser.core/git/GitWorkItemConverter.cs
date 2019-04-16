using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core.git
{
    public class GitWorkItemConverter : IWorkItemConverter
    {
        private readonly ICommandLineParser commandLineParser;
        private readonly IProcessWrapper processWrapper;
        private readonly IGitLogParser gitLogParser;

        private readonly string gitShowCommand;

        public GitWorkItemConverter(ICommandLineParser commandLineParser, IProcessWrapper processWrapper, IGitLogParser gitLogParser)
        {
            this.commandLineParser = commandLineParser;
            this.processWrapper = processWrapper;
            this.gitLogParser = gitLogParser;

            this.gitShowCommand = "git show --pretty=fuller --date=iso --numstat {0}";
        }

        private IChangeset Convert(WorkItem item)
        {
            // TODO Check if changeset is in hash format

            var parsedCommand = commandLineParser.ParseCommandLine(string.Format(gitShowCommand, item.ChangesetId));
            var stream = processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
            var commits = gitLogParser.Parse(stream);
            if (!commits.Any())
                return null;
            return commits.Single();
        }

        public IDictionary<DateTime, IDictionary<string, IChangeset>> Convert(IDictionary<DateTime, IDictionary<string, WorkItem>> items)
        {
            var dict = new Dictionary<DateTime, IDictionary<string, IChangeset>>();

            foreach(var workItemsByCloseDate in items.Values)
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
