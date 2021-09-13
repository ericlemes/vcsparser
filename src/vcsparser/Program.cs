using CommandLine;
using vcsparser.core;
using vcsparser.core.git;
using vcsparser.core.p4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database;
using vcsparser.core.Database.Cosmos;

namespace vcsparser
{
    class Program
    {
        static int Main(string[] args)
        {
            var parser = new Parser(config =>
            {
                config.HelpWriter = Console.Error;
                config.EnableDashDash = true;
            });
            var result = parser.ParseArguments<P4ExtractCommandLineArgs, GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs, DailyCodeChurnCommandLineArgs>(args)
                .MapResult(
                    (P4ExtractCommandLineArgs a) => RunPerforceCodeChurnProcessor(a),
                    (GitExtractCommandLineArgs a) => RunGitCodeChurnProcessor(a),
                    (SonarGenericMetricsCommandLineArgs a) => RunSonarGenericMetrics(a),
                    (DailyCodeChurnCommandLineArgs a) => RunDailyCodeChurn(a),
                    err => 1); 
            return result;
        }

        private static int RunPerforceCodeChurnProcessor(P4ExtractCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var changesParser = new ChangesParser();
            var describeParser = new DescribeParser();
            var commandLineParser = new CommandLineParser();
            var logger = new ConsoleLoggerWithTimestamp();
            var stopWatch = new StopWatchWrapper();
            var outputProcessor = GetOutputProcessorBasedOnOutputType(logger, a.OutputType, a.OutputFile, a.CosmosEndpoint, a.CosmosDbKey, a.DatabaseId, a.CodeChurnCosmosContainer, a.CosmosProjectName);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<WorkItem>(new FileStreamFactory());
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, webRequest, fileSystem, jsonParser, logger);
            var processor = new PerforceCodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, bugDatabaseProcessor, logger, stopWatch, outputProcessor, a);

            processor.QueryBugDatabase();
            return processor.Extract();
        }

        private static int RunGitCodeChurnProcessor(GitExtractCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var commandLineParser = new CommandLineParser();
            var gitLogParser = new GitLogParser();
            var logger = new ConsoleLoggerWithTimestamp();
            var outputProcessor = GetOutputProcessorBasedOnOutputType(logger, a.OutputType, a.OutputFile, a.CosmosEndpoint, a.CosmosDbKey, a.DatabaseId, a.CodeChurnCosmosContainer, a.CosmosProjectName);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<WorkItem>(new FileStreamFactory());
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, webRequest, fileSystem, jsonParser, logger);
            var processor = new GitCodeChurnProcessor(commandLineParser, processWrapper, gitLogParser, outputProcessor, bugDatabaseProcessor, logger, a);

            processor.QueryBugDatabase();
            return processor.Extract();
        }

        private static int RunSonarGenericMetrics(SonarGenericMetricsCommandLineArgs a)
        {
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<DailyCodeChurn>(new FileStreamFactory());
            var converters = new MeasureConverterListBuilder(new EnvironmentImpl()).Build(a);
            var jsonExporter = new JsonExporter(new FileStreamFactory());

            var processor = new SonarGenericMetricsProcessor(fileSystem, jsonParser, converters, jsonExporter, new ConsoleLoggerWithTimestamp());
            processor.Process(a);

            return 0;
        }

        private static int RunDailyCodeChurn(DailyCodeChurnCommandLineArgs a)
        {
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<DailyCodeChurn>(new FileStreamFactory());
            var logger = new ConsoleLoggerWithTimestamp();
            var exclusionsProcessor = new ExclusionsProcessor(a.Exclusions);
            var inclusionsProcessor = new InclusionsProcessor(a.Inclusions);
            var jsonExporter = new JsonExporter(new FileStreamFactory());

            var processor = new DailyCodeChurnProcessor(fileSystem, jsonParser, logger, exclusionsProcessor, inclusionsProcessor, jsonExporter);
            processor.Process(a);

            return 0;
        }

        private static IOutputProcessor GetOutputProcessorBasedOnOutputType(ILogger logger, OutputType outputType, string outputFile, string cosmosEndpoint, string cosmosDbKey, string cosmosDatabaseId, string codeChurnCosmosContainer, string cosmosProjectName)
        {
            if (outputType != OutputType.CosmosDb)
                return new JsonFilesOutputProcessor(new FileStreamFactory(), logger, new CodeChurnDataMapper(), outputType, outputFile);

            var cosmosConnection = new CosmosConnection(new DatabaseFactory(cosmosEndpoint, cosmosDbKey, null), cosmosDatabaseId);
            return new CosmosDbOutputProcessor(new JsonFilesOutputProcessor(new FileStreamFactory(), logger, new CodeChurnDataMapper(), outputType, outputFile),  logger, cosmosConnection, new CodeChurnDataMapper(), codeChurnCosmosContainer, cosmosProjectName);
        }
    }
}
