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
            var result = parser.ParseArguments<P4ExtractCommandLineArgs, GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs, BugDatabaseLineArgs>(args)
                .MapResult(
                    (P4ExtractCommandLineArgs a) => RunPerforceCodeChurnProcessor(a),
                    (GitExtractCommandLineArgs a) => RunGitCodeChurnProcessor(a),
                    (SonarGenericMetricsCommandLineArgs a) => RunSonarGenericMetrics(a),
                    (BugDatabaseLineArgs a) => RunBugDatabase(a),
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
            var processor = new PerforceCodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, logger, stopWatch, outputProcessor, a.BugRegexes);

            processor.Extract(a.OutputType, a.OutputFile, a.P4ChangesCommandLine, a.P4DescribeCommandLine);
            return 0;
        }

        private static int RunGitCodeChurnProcessor(GitExtractCommandLineArgs a)
        {
            var processor = new GitCodeChurnProcessor(new CommandLineParser(), new ProcessWrapper(), new GitLogParser(), new OutputProcessor(new FileStreamFactory(), new ConsoleLogger()), new ConsoleLogger(), a);
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

        private static int RunBugDatabase(BugDatabaseLineArgs a)
        {
            var processor = new BugDatabaseProcessor(new ConsoleLogger());
            int code = processor.Process(a);
            return code;
        }
    }
}
