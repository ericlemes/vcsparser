using System;
using System.Collections.Generic;
using vcsparser.core;

namespace vcsparser.core
{
    public class SonarGenericMetricsProcessor
    {
        private IFileSystem fileSystem;

        private IJsonParser<DailyCodeChurn> dailyCodeChurnParser;

        private List<IMeasureConverter> measureConverters;

        private IJsonExporter jsonExporter;

        private ILogger logger;

        public SonarGenericMetricsProcessor(IFileSystem fileSystem, IJsonParser<DailyCodeChurn> dailyCodeChurnParser, List<IMeasureConverter> measureConverters,
            IJsonExporter jsonExporter, ILogger logger)
        {
            this.fileSystem = fileSystem;
            this.dailyCodeChurnParser = dailyCodeChurnParser;
            this.measureConverters = measureConverters;
            this.jsonExporter = jsonExporter;
            this.logger = logger;
        }

        public void Process(SonarGenericMetricsCommandLineArgs a)
        {
            var files = fileSystem.GetFiles(a.InputDir, "*.json");
            SonarMeasuresJson outputJson = new SonarMeasuresJson();            

            foreach(var file in files)
            {
                this.logger.LogToConsole(String.Format("Processing {0}", file.FileName));
                var codeChurnList = this.dailyCodeChurnParser.ParseFile(file.FileName);
                foreach(var converter in measureConverters)
                {
                    foreach (var codeChurn in codeChurnList)
                        converter.ProcessFileMeasure(codeChurn, outputJson);
                    
                    converter.ProcessProjectMeasure(outputJson);
                }
            }

            this.jsonExporter.Export(outputJson, a.OutputFile);
        }
    }
}