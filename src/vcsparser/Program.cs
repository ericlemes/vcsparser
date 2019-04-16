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
            var result = parser.ParseArguments<P4ExtractCommandLineArgs, GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                    (P4ExtractCommandLineArgs a) => RunPerforceCodeChurnProcessor(a),
                    (GitExtractCommandLineArgs a) => RunGitCodeChurnProcessor(a),
                    (SonarGenericMetricsCommandLineArgs a) => RunSonarGenericMetrics(a),
                    err => 1);
            return result;
        }

        private static int RunPerforceCodeChurnProcessor(P4ExtractCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var changesParser = new ChangesParser();
            var describeParser = new DescribeParser();
            var commandLineParser = new CommandLineParser();
            var logger = new ConsoleLogger();
            var stopWatch = new StopWatchWrapper();
            var outputProcessor = new OutputProcessor(new FileStreamFactory(), logger);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var workItemConverter = new PerforceWorkItemConverter(commandLineParser, processWrapper, describeParser);
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, workItemConverter, webRequest);
            var processor = new PerforceCodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, bugDatabaseProcessor, logger, stopWatch, outputProcessor, a);

            processor.CollectBugDatabaseCache();
            processor.Extract();
            return 0;
        }

        private static int RunGitCodeChurnProcessor(GitExtractCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var commandLineParser = new CommandLineParser();
            var gitLogParser = new GitLogParser();
            var logger = new ConsoleLogger();
            var outputProcessor = new OutputProcessor(new FileStreamFactory(), logger);
            var bugDatabaseFactory = new BugDatabaseFactory();
            var bugDatabaseDllLoader = new BugDatabaseDllLoader(logger, bugDatabaseFactory);
            var workItemConverter = new GitWorkItemConverter(commandLineParser, processWrapper, gitLogParser);
            var webRequest = new WebRequest(new HttpClientWrapperFactory(bugDatabaseFactory));
            var bugDatabaseProcessor = new BugDatabaseProcessor(bugDatabaseDllLoader, workItemConverter, webRequest);
            var processor = new GitCodeChurnProcessor(commandLineParser, processWrapper, gitLogParser, outputProcessor, bugDatabaseProcessor, logger, a);

            processor.CollectBugDatabaseCache();
            processor.Extract();
            return 0;
        }

        private static int RunSonarGenericMetrics(SonarGenericMetricsCommandLineArgs a)
        {
            var fileSystem = new FileSystem();
            var jsonParser = new JsonDailyCodeChurnParser(new FileStreamFactory());
            var converters = new MeasureConverterListBuilder(new EnvironmentImpl()).Build(a);
            var jsonExporter = new JsonExporter(new FileStreamFactory());

            var processor = new SonarGenericMetricsProcessor(fileSystem, jsonParser, converters, jsonExporter, new ConsoleLogger());
            processor.Process(a);

            return 0;
        }
    }
}
