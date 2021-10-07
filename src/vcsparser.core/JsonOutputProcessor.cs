using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace vcsparser.core
{
    public class JsonOutputProcessor : IOutputProcessor
    {
        private readonly IDataConverter dataConverter;
        private readonly IStreamFactory streamFactory;
        private readonly ILogger logger;

        public JsonOutputProcessor(IDataConverter dataConverter, IStreamFactory streamFactory, ILogger logger)
        {
            this.streamFactory = streamFactory;
            this.logger = logger;
            this.dataConverter = dataConverter;
        }

        public void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (outputType == OutputType.SingleFile)
                ProcessOutputSingleFile(outputFile, dict);
            else if (outputType == OutputType.SeparateFiles)
                ProcessOutputSeparateFiles(outputFile, dict);
            else if (outputType == OutputType.MultipleFile)
                ProcessOutputMultipleFile(outputFile, dict);
        }

        public void ProcessOutputMultipleFile<T>(string filePrefix, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfLists = dataConverter.ConvertDictToOrderedListPerDay<T>(dict);
            logger.LogToConsole(listOfLists.Count + " files to output");
            foreach (var list in listOfLists)
                ProcessOutputSingleFile(filePrefix + "_" + list.Key.ToString("yyyy-MM-dd") + ".json", list.Value.Values);
        }

        public void ProcessOutputSingleFile<T>(string fileName, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var result = dataConverter.ConvertDictToOrderedList<T>(dict);
            ProcessOutputSingleFile(fileName, result);
        }

        public void ProcessOutputSeparateFiles<T>(string filePrefix, Dictionary<DateTime, Dictionary<string, T>> dict)
            where T : IOutputJson
        {
            var separateFilesList = dict.SelectMany(x => x.Value.Select(y => y.Value));

            foreach (var separateFile in separateFilesList)
            {
                var fileName = $"{filePrefix}_{separateFile.GetFileLongName()}.json";
                ProcessOutputSingleFile(fileName, new List<T> { separateFile });
            }
        }

        private void ProcessOutputSingleFile<T>(string fileName, IList<T> result) where T : IOutputJson
        {
            var fs = streamFactory.createFileStream(fileName, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(fs);

            var jsonSerializer = JsonSerializer.Create();
            jsonSerializer.Converters.Add(new JsonDateTimeCustomConverter(DailyCodeChurn.DATE_FORMAT, CultureInfo.InvariantCulture));
            using (sw)
            {
                logger.LogToConsole("Writing json to " + fileName);

                var output = new JsonOutputData<T>
                {
                    SchemaVersion = JsonOutputData<T>.CurrentVersion,
                    Data = result
                };

                jsonSerializer.Serialize(sw, output);
                sw.Flush();
            }
        }
    }
}
