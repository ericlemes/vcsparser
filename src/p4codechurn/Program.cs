using CommandLine;
using p4codechurn.core;
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
        static void Main(string[] args)
        {
        var result = Parser.Default.ParseArguments<ExtractCommandLineArgs, SonarGenericMetricsCommandLineArgs>(args)
                .MapResult(
                    (ExtractCommandLineArgs a) => RunCodeChurnProcessor(a),
                    (SonarGenericMetricsCommandLineArgs a) => RunSonarGenericMetrics(a),
                    err => 1);            
        }

        private static int RunCodeChurnProcessor(ExtractCommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var changesParser = new ChangesParser();
            var describeParser = new DescribeParser();
            var commandLineParser = new CommandLineParser();
            var logger = new ConsoleLogger();
            var stopWatch = new StopWatchWrapper();
            var outputProcessor = new OutputProcessor(new FileStreamFactory(), logger);
            var processor = new CodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, logger, stopWatch, outputProcessor);

            processor.Extract(a.OutputType, a.OutputFile, a.P4ChangesCommandLine, a.P4DescribeCommandLine);
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
