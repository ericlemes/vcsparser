using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.git;
using vcsparser.core.p4;

namespace vcsparser.core.bugdatabase
{
    public class GetChangeset
    {
        private IProcessWrapper processWrapper;

        private IDescribeParser describeParser;
        private IGitLogParser gitLogParser;

        private ICommandLineParser commandLineParser;

        public string PerforceDescribeCommand { get; set; }
        public string GitLogCommand { get; set; }

        public GetChangeset(IProcessWrapper processWrapper, IDescribeParser describeParser, IGitLogParser gitLogParser, ICommandLineParser commandLineParser)
        {
            this.processWrapper = processWrapper;
            this.describeParser = describeParser;
            this.gitLogParser = gitLogParser;
            this.commandLineParser = commandLineParser;
            this.PerforceDescribeCommand = "p4 describe -ds {0}";
            this.GitLogCommand = "git log --pretty=fuller --date=iso --after={0} --numstat";
        }

        public PerforceChangeset ProcessPerforceRecord(int Changeset)
        {
            try
            {
                var parsedCommandLine = commandLineParser.ParseCommandLine(string.Format(PerforceDescribeCommand, Changeset));
                var stream = processWrapper.Invoke(parsedCommandLine.Item1, parsedCommandLine.Item2);
                var changeset = describeParser.Parse(stream);
                return changeset;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<GitCommit> ProcessGitRecord(DateTime date)
        {
            try
            {
                var parsedCommand = commandLineParser.ParseCommandLine(string.Format(this.GitLogCommand, date.ToString("yyyy-MM-dd")));
                var stream = processWrapper.Invoke(parsedCommand.Item1, parsedCommand.Item2);
                var commits = gitLogParser.Parse(stream);
                return commits;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
