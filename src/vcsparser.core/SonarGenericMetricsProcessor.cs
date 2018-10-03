using System;
using System.Collections.Generic;
using p4codechurn.core;

namespace p4codechurn.core
{
    public class SonarGenericMetricsProcessor
    {
        private IFileSystem fileSystem;

        private ICsvParser csvParser;

        private List<IMeasureConverter> measureConverters;

        private IJsonExporter jsonExporter;

        private ILogger logger;

        public SonarGenericMetricsProcessor(IFileSystem fileSystem, ICsvParser csvParser, List<IMeasureConverter> measureConverters,
            IJsonExporter jsonExporter, ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.csvParser = csvParser;
            this.measureConverters = measureConverters;
            this.jsonExporter = jsonExporter;
            this.logger = logger;
        }

        public void Process(SonarGenericMetricsCommandLineArgs a)
        {
            var files = fileSystem.GetFiles(a.InputDir, "*.csv");
            SonarMeasuresJson outputJson = new SonarMeasuresJson();            

            foreach(var file in files)
            {
                this.logger.LogToConsole(String.Format("Processing {0}", file.FileName));
                var codeChurnList = this.csvParser.ParseFile(file.FileName);
                foreach(var converter in measureConverters)
                {
                    foreach (var codeChurn in codeChurnList)
                        converter.Process(codeChurn, outputJson);
                }
            }

            this.jsonExporter.Export(outputJson, a.OutputFile);
        }
    }
}