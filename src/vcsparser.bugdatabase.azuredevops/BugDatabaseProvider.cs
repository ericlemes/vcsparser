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
using System.Globalization;

namespace vcsparser.bugdatabase
{
    public class BugDatabaseProvider : IBugDatabaseProvider
    {
        public ILogger logger;
        private IWebRequest webRequest;

        private IAzureDevOpsFactory azureDevOpsFactory = new AzureDevOpsFactory();

        private IAzureDevOps azureDevOps;

        public void SetLogger(ILogger logger)
        {
            this.logger = logger;
        }

        public void SetWebRequest(IWebRequest webRequest)
        {
            this.webRequest = webRequest;
        }

        public void SetAzureDevOpsFactory(IAzureDevOpsFactory azureDevOpsFactory)
        {
            this.azureDevOpsFactory = azureDevOpsFactory;
        }

        public int ProcessArgs(IEnumerable<string> args)
        {
            var code = Parser.Default.ParseArguments<DllArgs>(args).MapResult(
                (DllArgs a) => SetUp(a),
                err => 1);
            return code;
        }

        private int SetUp(DllArgs args)
        {
            const string dateFormat = @"yyyy-MM-dd";
            if (!DateTime.TryParseExact(args.From, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _) ||
                !DateTime.TryParseExact(args.To, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
            {
                logger.LogToConsole($"Date inputs must match '{dateFormat}'");
                return 1;
            }

            IAzureDevOpsRequest request = new AzureDevOpsRequest(webRequest, args);
            IApiConverter converter = new ApiConverter();
            ITimeKeeper timeKeeper = new TimeKeeper(TimeSpan.FromSeconds(30));
            azureDevOps = azureDevOpsFactory.GetAzureDevOps(logger, request, converter, timeKeeper);
            return 0;
        }

        public WorkItemList Process()
        {
            if (azureDevOps == null) return null;
            return azureDevOps.GetWorkItems();
        }
    }
}
