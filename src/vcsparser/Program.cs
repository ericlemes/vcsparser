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
using vcsparser.core.Database.Repository;
using vcsparser.core.Factory;

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
            var result = parser.ParseArguments<P4ExtractCommandLineArgs, GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs, DailyCodeChurnCommandLineArgs, GitExtractToCosmosDbCommandLineArgs, DownloadFromCosmosDbCommandLineArgs, SonarGenericMetricsCosmosDbCommandLineArgs, DataFromFilesToCosmosDbCommandLineArgs, P4ExtractToCosmosDbCommandLineArgs>(args)
                .MapResult(
                    (P4ExtractCommandLineArgs a) => RunPerforceCodeChurnProcessor(a),
                    (GitExtractCommandLineArgs a) => RunGitCodeChurnProcessor(a),
                    (SonarGenericMetricsCommandLineArgs a) => RunSonarGenericMetrics(a),
                    (DailyCodeChurnCommandLineArgs a) => RunDailyCodeChurn(a),
                    (GitExtractToCosmosDbCommandLineArgs a) => RunGitToCosmosDbCodeChurnProcessor(a),
                    (DownloadFromCosmosDbCommandLineArgs a) => RunDownloadCodeChurnFromCosmosDbToJsonFiles(a),
                    (SonarGenericMetricsCosmosDbCommandLineArgs a) => RunSonarGenericMetricsFromCosmosDb(a),
                    (DataFromFilesToCosmosDbCommandLineArgs a) => RunCodeChurnFromFiles(a),
                    (P4ExtractToCosmosDbCommandLineArgs a) => RunPerforceToCosmosDbCodeChurnProcessor(a),
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
            var outputProcessor = new JsonOutputProcessor(new DataConverter(), new FileStreamFactory(), logger);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<WorkItem>(new FileStreamFactory());
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, webRequest, fileSystem, jsonParser, logger, a.BugDatabaseOutputFile);
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
            var outputProcessor = new JsonOutputProcessor(new DataConverter(), new FileStreamFactory(), logger);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<WorkItem>(new FileStreamFactory());
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, webRequest, fileSystem, jsonParser, logger, a.BugDatabaseOutputFile);
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

        private static int RunGitToCosmosDbCodeChurnProcessor(GitExtractToCosmosDbCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var commandLineParser = new CommandLineParser();
            var gitLogParser = new GitLogParser();
            var logger = new ConsoleLoggerWithTimestamp();
            var cosmosConnection = new CosmosConnection(new DatabaseFactory(a, JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB()), a.DatabaseId, Properties.Settings.Default.CosmosBulkBatchSize);
            var dataDocumentRepository = new DataDocumentRepository(cosmosConnection, a.CodeChurnCosmosContainer);
            var cosmosOutputProcessor = new CosmosDbOutputProcessor(logger, dataDocumentRepository, new DataConverter(), a.CosmosProjectName, Properties.Settings.Default.CosmosBulkBatchSize);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();
            var jsonParser = new JsonListParser<WorkItem>(new FileStreamFactory());
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, webRequest, fileSystem, jsonParser, logger, string.Empty);
            //, a.BugDatabaseOutputFile

            var processor = new GitCodeChurnProcessor(commandLineParser, processWrapper, gitLogParser, cosmosOutputProcessor, bugDatabaseProcessor, logger, a);
            processor.QueryBugDatabase();

            return processor.Extract();
        }

        private static int RunDownloadCodeChurnFromCosmosDbToJsonFiles(DownloadFromCosmosDbCommandLineArgs a)
        {
            var logger = new ConsoleLoggerWithTimestamp();
            var cosmosConnection = new CosmosConnection(new DatabaseFactory(a, JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB()), a.DatabaseId, Properties.Settings.Default.CosmosBulkBatchSize);
            var dataDocumentRepository = new DataDocumentRepository(cosmosConnection, a.CodeChurnCosmosContainer);
            var cosmosOutputProcessor = new CosmosDbOutputProcessor(logger, dataDocumentRepository, new DataConverter(), a.CosmosProjectName, Properties.Settings.Default.CosmosBulkBatchSize);
            var jsonOutputProcessor = new JsonOutputProcessor(new DataConverter(), new FileStreamFactory(), logger);
            var environment = new EnvironmentImpl();

            var endDate = a.EndDate ?? (a.EndDate = environment.GetCurrentDateTime());

            switch (a.DocumentType)
            {
                case DocumentType.BugDatabase:
                {
                    var data = cosmosOutputProcessor.GetDocumentsInDateRange<WorkItem>(a.StartDate.Value, endDate.Value);
                    jsonOutputProcessor.ProcessOutput(a.OutputType, a.OutputFile, data);
                    break;
                }
                case DocumentType.CodeChurn:
                {
                    var data = cosmosOutputProcessor.GetDocumentsInDateRange<DailyCodeChurn>(a.StartDate.Value, endDate.Value);
                    jsonOutputProcessor.ProcessOutput(a.OutputType, a.OutputFile, data);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0;
        }


        private static int RunSonarGenericMetricsFromCosmosDb(SonarGenericMetricsCosmosDbCommandLineArgs a)
        {
            var logger = new ConsoleLoggerWithTimestamp();
            var jsonParser = new JsonListParser<DailyCodeChurn>(new FileStreamFactory());
            var jsonExporter = new JsonExporter(new FileStreamFactory());

            var cosmosConnection = new CosmosConnection(new DatabaseFactory(a, JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB()), a.DatabaseId, Properties.Settings.Default.CosmosBulkBatchSize);
            var dataDocumentRepository = new DataDocumentRepository(cosmosConnection, a.CodeChurnCosmosContainer);
            var cosmosOutputProcessor = new CosmosDbOutputProcessor(logger, dataDocumentRepository, new DataConverter(), a.CosmosProjectName, Properties.Settings.Default.CosmosBulkBatchSize);
            var environment = new EnvironmentImpl();

            var converters = new MeasureConverterListBuilder(environment).Build(a);

            Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> data;

            if (a.StartDate == null && a.EndDate == null)
                data = cosmosOutputProcessor.GetAllDocumentsByProjectNameAndDocumentType<DailyCodeChurn>();
            else
            {
                if (a.StartDate == null)
                    throw new ArgumentNullException(nameof(a.StartDate));
                if (a.EndDate == null)
                    a.EndDate = environment.GetCurrentDateTime();
                data = cosmosOutputProcessor.GetDocumentsInDateRange<DailyCodeChurn>(a.StartDate.Value, a.EndDate.Value);
            }

            var processor = new SonarGenericMetricsProcessor(jsonParser, converters, jsonExporter,
                new ConsoleLoggerWithTimestamp(), new DataConverter());

            processor.Process(a, data);
            return 0;
        }

        private static int RunCodeChurnFromFiles(DataFromFilesToCosmosDbCommandLineArgs a)
        {
            var fileSystem = new FileSystem();
            var logger = new ConsoleLoggerWithTimestamp();
            var cosmosConnection = new CosmosConnection(new DatabaseFactory(a, JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB()), a.DatabaseId, Properties.Settings.Default.CosmosBulkBatchSize);
            var dataDocumentRepository = new DataDocumentRepository(cosmosConnection, a.CodeChurnCosmosContainer);
            var cosmosOutputProcessor = new CosmosDbOutputProcessor(logger, dataDocumentRepository, new DataConverter(), a.CosmosProjectName, Properties.Settings.Default.CosmosBulkBatchSize);

            IDataFromFileToCosmosDb codeChurnFromFileToCosmosDb;

            if (a.DocumentType == DocumentType.CodeChurn)
                codeChurnFromFileToCosmosDb = new DataFromFileToCosmosDb<DailyCodeChurn>(logger, fileSystem, cosmosOutputProcessor, new JsonListParser<DailyCodeChurn>(new FileStreamFactory()), a.Path, a.CosmosProjectName);
            else
                codeChurnFromFileToCosmosDb = new DataFromFileToCosmosDb<WorkItem>(logger, fileSystem, cosmosOutputProcessor, new JsonListParser<WorkItem>(new FileStreamFactory()), a.Path, a.CosmosProjectName);

            var output = codeChurnFromFileToCosmosDb.Extract();

            return output;
        }

        private static int RunPerforceToCosmosDbCodeChurnProcessor(P4ExtractToCosmosDbCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var changesParser = new ChangesParser();
            var describeParser = new DescribeParser();
            var commandLineParser = new CommandLineParser();
            var logger = new ConsoleLoggerWithTimestamp();
            var stopWatch = new StopWatchWrapper();
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var fileSystem = new FileSystem();

            var cosmosConnection = new CosmosConnection(new DatabaseFactory(a, JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB()), a.DatabaseId, Properties.Settings.Default.CosmosBulkBatchSize);
            var dataDocumentRepository = new DataDocumentRepository(cosmosConnection, a.CodeChurnCosmosContainer);
            var cosmosOutputProcessor = new CosmosDbOutputProcessor(logger, dataDocumentRepository, new DataConverter(), a.CosmosProjectName, Properties.Settings.Default.CosmosBulkBatchSize);

            var bugDatabaseProcessor = new CosmosDbBugDatabaseProcessor(bugDatabaseDllLoader, fileSystem, webRequest, logger);
            var processor = new PerforceCodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, bugDatabaseProcessor, logger, stopWatch, cosmosOutputProcessor, a);

            processor.QueryBugDatabase();
            return processor.Extract();
        }
    }
}
