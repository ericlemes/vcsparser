using CommandLine;
using p4codechurn.core;
using p4codechurn.core.git;
using p4codechurn.core.p4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn
{
    class Program
    {
        static int Main(string[] args)
        {
            var result = Parser.Default.ParseArguments<P4ExtractCommandLineArgs, GitExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
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
            var processor = new PerforceCodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, logger, stopWatch, outputProcessor);

            processor.Extract(a.OutputType, a.OutputFile, a.P4ChangesCommandLine, a.P4DescribeCommandLine);
                return 0;
        }

        private static int RunGitCodeChurnProcessor(GitExtractCommandLineArgs a)
        {            
            var processor = new GitCodeChurnProcessor(new CommandLineParser(), new ProcessWrapper(), new GitLogParser(), new OutputProcessor(new FileStreamFactory(), new ConsoleLogger()), new ConsoleLogger());
            processor.Extract(a);
            return 0;
        }

        private static int RunSonarGenericMetrics(SonarGenericMetricsCommandLineArgs a)
        {
            var fileSystem = new FileSystem();
            var csvParser = new CsvParser(new FileStreamFactory());
            var converters = new MeasureConverterListBuilder(new EnvironmentImpl()).Build(a);
            var jsonExporter = new JsonExporter(new FileStreamFactory());

            var processor = new SonarGenericMetricsProcessor(fileSystem, csvParser, converters, jsonExporter, new ConsoleLogger());
            processor.Process(a);

            return 0;
        }


    }
}
