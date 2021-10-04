using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace vcsparser.core
{
    public class SonarGenericMetricsProcessor
    {
        private readonly IFileSystem fileSystem;

        private readonly IJsonListParser<DailyCodeChurn> dailyCodeChurnParser;

        private readonly List<IMeasureConverter> measureConverters;

        private readonly IJsonExporter jsonExporter;

        private readonly ILogger logger;

        public SonarGenericMetricsProcessor(IFileSystem fileSystem, IJsonListParser<DailyCodeChurn> dailyCodeChurnParser, List<IMeasureConverter> measureConverters,
            IJsonExporter jsonExporter, ILogger logger) : this(dailyCodeChurnParser, measureConverters, jsonExporter, logger)
        {
            this.fileSystem = fileSystem;
        }

        public SonarGenericMetricsProcessor(IJsonListParser<DailyCodeChurn> dailyCodeChurnParser, List<IMeasureConverter> measureConverters,
            IJsonExporter jsonExporter, ILogger logger)
        {
            this.dailyCodeChurnParser = dailyCodeChurnParser;
            this.measureConverters = measureConverters;
            this.jsonExporter = jsonExporter;
            this.logger = logger;
        }

        public void Process(SonarGenericMetricsCommandLineArgs a)
        {
            var files = fileSystem.GetFiles(a.InputDir, "*.json");
            var outputJson = new SonarMeasuresJson();            

            foreach(var file in files)
            {
                this.logger.LogToConsole($"Processing {file.FileName}");
                var codeChurnList = this.dailyCodeChurnParser.ParseFile(file.FileName);
                
                ProcessDailyCodeChurnList(codeChurnList, outputJson);
            }

            this.jsonExporter.Export(outputJson, a.OutputFile);
        }
   

        public void Process(SonarGenericMetricsCosmosDbCommandLineArgs a, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> data)
        {
            var outputJson = new SonarMeasuresJson();
            var documentsPerDay = data
                .Select(x => x.Value)
                .Reverse()
                .ToList();

            for (var i = documentsPerDay.Count - 1; i >= 0; i--)
            {
                var documentPerDay = documentsPerDay[i];

                var sortedValues = (from entry in documentPerDay orderby entry.Value.FileName select entry).ToList();
                documentPerDay.Clear();

                foreach (var sortedValue in sortedValues)
                    documentPerDay.Add(sortedValue.Key, sortedValue.Value);

            }

            foreach (var document in documentsPerDay)
            {
                this.logger.LogToConsole($"Processing {document}");
                var codeChurnList = document.Values.ToList();

                ProcessDailyCodeChurnList(codeChurnList, outputJson);
            }

            this.jsonExporter.Export(outputJson, a.OutputFile);
        }

        private void ProcessDailyCodeChurnList(IList<DailyCodeChurn> codeChurnList, SonarMeasuresJson outputJson)
        {
            foreach (var converter in measureConverters)
            {
                foreach (var codeChurn in codeChurnList)
                    converter.ProcessFileMeasure(codeChurn, outputJson);

                converter.ProcessProjectMeasure(outputJson);
            }
        }
    }
}