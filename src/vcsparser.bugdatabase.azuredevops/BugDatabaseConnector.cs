using System;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.bugdatabase.azuredevops;
using System.Text.RegularExpressions;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.p4;
using vcsparser.core.git;

namespace vcsparser.bugdatabase
{
    public class BugDatabaseConnector : BugDatabaseProvider
    {
        private AzureDevOps azureDevOps;

        public BugDatabaseConnector(ILogger logger) : base(logger) { }

        public override int ProcessArgs(string[] args)
        {
            var code = Parser.Default.ParseArguments<DllArgs>(args).MapResult(
                (DllArgs a) => SetUp(a),
                err => 1);
            return code;
        }

        private int SetUp(DllArgs args)
        {
            // TODO (Argument validation check) Decide if we need to do this or not?
            Regex date = new Regex(@"^\d{4}-[01]\d-[0-3]\d$");
            if (!date.IsMatch(args.From) || !date.IsMatch(args.To))
            {
                logger.LogToConsole("Data inputs must match 'yyyy-mm-dd'");
                return 1;
            }

            var getChangeset = new GetChangeset(new ProcessWrapper(), new DescribeParser(), new GitLogParser(), new CommandLineParser());
            var webRequest = new WebRequest();
            azureDevOps = new AzureDevOps(logger, getChangeset, webRequest, args);
            return 0;
        }

        public override WorkItemList Run()
        {
            if (azureDevOps == null) return null;
            return azureDevOps.GetWorkItems();
        }
    }
}
