using System;
using CommandLine;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.bugdatabase.azuredevops;
using System.Text.RegularExpressions;

namespace vcsparser.bugdatabase
{
    public class BugDatabaseProvider : IBugDatabaseProvider
    {
        private AzureDevOps azureDevOps;

        public bool ProcessArgs(string[] args)
        {
            bool parsed = false;
            Parser.Default.ParseArguments<AzureDevOpsArgs>(args).WithParsed((opts) =>
            {
                // TODO Argument validation check
                if (!isValidDateFormat(opts.From) || !isValidDateFormat(opts.To))
                {
                    parsed = false;
                    Console.WriteLine("Data inputs must match 'yyyy-mm-dd'");
                    return;
                }

                azureDevOps = new AzureDevOps(opts.Organisation, opts.Project, opts.Team, opts.PersonalAccessToken, opts.QueryString, opts.From, opts.To);
                parsed = true;
            }).WithNotParsed((errs) =>
            {
                parsed = false;
            });
            return parsed;
        }

        public WorkItemList Run()
        {
            if (azureDevOps == null) return null;
            return azureDevOps.Query();
        }

        private bool isValidDateFormat(string date)
        {
            Regex r = new Regex(@"^\d{4}-[01]\d-[0-3]\d$");
            return r.IsMatch(date);
        }
    }
}
