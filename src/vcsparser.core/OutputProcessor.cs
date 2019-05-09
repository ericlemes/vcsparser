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
    public class OutputProcessor : IOutputProcessor
    {
        private IStreamFactory streamFactory;
        private ILogger logger;

        public OutputProcessor(IStreamFactory streamFactory, ILogger logger)
        {
            this.streamFactory = streamFactory;
            this.logger = logger;
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
                jsonSerializer.Serialize(sw, result);
                sw.Flush();
            }
        }

        private SortedList<DateTime, SortedList<DailyCodeChurn, DailyCodeChurn>> ConvertCodeChurnDictToOrderedListPerDay(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            SortedList<DateTime, SortedList<DailyCodeChurn, DailyCodeChurn>> result = new SortedList<DateTime, SortedList<DailyCodeChurn, DailyCodeChurn>>();
            foreach (var a in dict)
            {
                SortedList<DailyCodeChurn, DailyCodeChurn> dailyList = new SortedList<DailyCodeChurn, DailyCodeChurn>();
                foreach (var b in a.Value)
                    if (b.Value.TotalLinesChanged > 0)
                        dailyList.Add(b.Value, b.Value);
                if (dailyList.Count > 0)
                    result.Add(a.Key, dailyList);
            }
            return result;
        }

        private IList<DailyCodeChurn> ConvertCodeChurnDictToOrderedList(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            SortedList<DailyCodeChurn, DailyCodeChurn> sortedList = new SortedList<DailyCodeChurn, DailyCodeChurn>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (b.Value.TotalLinesChanged > 0)
                        sortedList.Add(b.Value, b.Value);
            return sortedList.Values;
        }

        private SortedList<DateTime, SortedList<WorkItem, WorkItem>> ConvertBugDatabaseDictToOrderedListPerDay(Dictionary<DateTime, Dictionary<string, WorkItem>> dict)
        {
            var result = new SortedList<DateTime, SortedList<WorkItem, WorkItem>>();
            foreach (var a in dict)
            {
                SortedList<WorkItem, WorkItem> dailyList = new SortedList<WorkItem, WorkItem>();
                foreach (var b in a.Value)
                    if (!string.IsNullOrWhiteSpace(b.Value.ChangesetId))
                        dailyList.Add(b.Value, b.Value);
                if (dailyList.Count > 0)
                    result.Add(a.Key, dailyList);
            }
            return result;
        }

        private IList<WorkItem> ConvertBugDatabaseDictToOrderedList(Dictionary<DateTime, Dictionary<string, WorkItem>> dict)
        {
            SortedList<WorkItem, WorkItem> sortedList = new SortedList<WorkItem, WorkItem>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (!string.IsNullOrWhiteSpace(b.Value.ChangesetId))
                        sortedList.Add(b.Value, b.Value);
            return sortedList.Values;
        }

        private IList<T> ConvertDictToOrderedList<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (typeof(T) == typeof(DailyCodeChurn))
                return ConvertCodeChurnDictToOrderedList(dict as Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>) as IList<T>;
            if (typeof(T) == typeof(WorkItem))
                return ConvertBugDatabaseDictToOrderedList(dict as Dictionary<DateTime, Dictionary<string, WorkItem>>) as IList<T>;
            return new List<T>();
        }

        private SortedList<DateTime, SortedList<T, T>> ConvertDictToOrderedListPerDay<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (typeof(T) == typeof(DailyCodeChurn))
                return ConvertCodeChurnDictToOrderedListPerDay(dict as Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>) as SortedList<DateTime, SortedList<T, T>>;
            if (typeof(T) == typeof(WorkItem))
                return ConvertBugDatabaseDictToOrderedListPerDay(dict as Dictionary<DateTime, Dictionary<string, WorkItem>>) as SortedList<DateTime, SortedList<T, T>>;
            return new SortedList<DateTime, SortedList<T, T>>();
        }

        public void ProcessOutputMultipleFile<T>(string filePrefix, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var listOfLists = ConvertDictToOrderedListPerDay<T>(dict);
            logger.LogToConsole(listOfLists.Count + " files to output");
            foreach (var list in listOfLists)
                ProcessOutputSingleFile<T>(filePrefix + "_" + list.Key.ToString("yyyy-MM-dd") + ".json", list.Value.Values);
        }

        public void ProcessOutputSingleFile<T>(string fileName, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            var result = ConvertDictToOrderedList<T>(dict);
            ProcessOutputSingleFile<T>(fileName, result);
        }

        public void ProcessOutput<T>(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (outputType == OutputType.SingleFile)
                ProcessOutputSingleFile<T>(outputFile, dict);
            else
                ProcessOutputMultipleFile<T>(outputFile, dict);
        }
    }
}
