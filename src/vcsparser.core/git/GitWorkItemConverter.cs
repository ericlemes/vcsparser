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

        public IEnumerable<IChangeset> Convert(IEnumerable<WorkItem> items)
        {
            foreach (var item in items)
            {
                var changeset = Convert(item);
                if (changeset != null)
                    yield return changeset;
            }
        }
    }
}
