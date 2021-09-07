using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public class JsonFilesOutputProcessor : OutputProcessor
    {
        private IStreamFactory streamFactory;
        private ILogger logger;

        public JsonFilesOutputProcessor(IStreamFactory streamFactory, ILogger logger)
        {
            this.streamFactory = streamFactory;
            this.logger = logger;
        }

        public override void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict)
        {
            if (outputType == OutputType.SingleFile)
                ProcessOutputSingleFile(outputFile, dict);
            else
                ProcessOutputMultipleFile(outputFile, dict);
        }

        public void ProcessOutputMultipleFile<T>(string filePrefix, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfLists = ConvertDictToOrderedListPerDay<T>(dict);
            logger.LogToConsole(listOfLists.Count + " files to output");
            foreach (var list in listOfLists)
                ProcessOutputSingleFile(filePrefix + "_" + list.Key.ToString("yyyy-MM-dd") + ".json", list.Value.Values);
        }

        public void ProcessOutputSingleFile<T>(string fileName, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var result = ConvertDictToOrderedList<T>(dict);
            ProcessOutputSingleFile(fileName, result);
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
