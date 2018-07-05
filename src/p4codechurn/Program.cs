using CommandLine;
using CsvHelper;
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
            var result = Parser.Default.ParseArguments<CommandLineArgs>(args)
                .WithParsed<CommandLineArgs>(a => RunCodeChurnProcessor(a));            
        }

        private static void RunCodeChurnProcessor(CommandLineArgs a)
        {
            var processWrapper = new ProcessWrapper();
            var changesParser = new ChangesParser();
            var describeParser = new DescribeParser();
            var commandLineParser = new CommandLineParser();
            var logger = new ConsoleLogger();
            var stopWatch = new StopWatchWrapper();
            var processor = new CodeChurnProcessor(processWrapper, changesParser, describeParser, commandLineParser, logger, stopWatch);

            var result = processor.Process(a.P4ChangesCommandLine, a.P4DescribeCommandLine);

            var fs = new FileStream(a.OutputFile, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            var sw = new StreamWriter(fs);
            var csvWriter = new CsvWriter(sw);
            using (fs)
            {
                using (sw)
                {
                    logger.LogToConsole("Writing csv to " + a.OutputFile);
                    csvWriter.WriteRecords(result);
                }
            }
        }
    }
}
