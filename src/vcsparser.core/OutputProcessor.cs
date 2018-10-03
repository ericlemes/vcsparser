﻿using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace p4codechurn.core
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

        public void ProcessOutputSingleFile(string fileName, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            var result = ConvertDictToOrderedList(dict);
            ProcessOutputSingleFile(fileName, result);
        }

        public void ProcessOutputSingleFile(string fileName, IList<DailyCodeChurn> result)
        {
            var fs = streamFactory.createFileStream(fileName, FileMode.Create, FileAccess.Write);
            var sw = new StreamWriter(fs);

            var csvWriter = new CsvWriter(sw);
            csvWriter.Configuration.HasHeaderRecord = true;                        
            using (sw)
            {
                logger.LogToConsole("Writing csv to " + fileName);
                csvWriter.WriteRecords(result);
                csvWriter.Flush();                    
                sw.Flush();                    
            }            
        }

        public void ProcessOutputMultipleFile(string filePrefix, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {            
            var listOfLists = ConvertDictToOrderedListPerDay(dict);
            logger.LogToConsole(listOfLists.Count + " files to output");
            foreach (var list in listOfLists)
                ProcessOutputSingleFile(filePrefix + "_" + list.Key.ToString("yyyy-MM-dd") + ".csv", list.Value.Values);
        }

        private SortedList<DateTime, SortedList<DailyCodeChurn, DailyCodeChurn>> ConvertDictToOrderedListPerDay(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
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

        private IList<DailyCodeChurn> ConvertDictToOrderedList(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            SortedList<DailyCodeChurn, DailyCodeChurn> sortedList = new SortedList<DailyCodeChurn, DailyCodeChurn>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (b.Value.TotalLinesChanged > 0)
                        sortedList.Add(b.Value, b.Value);
            return sortedList.Values;
        }

        public void ProcessOutput(OutputType outputType, string outputFile, Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {            
            if (outputType == OutputType.SingleFile)
                ProcessOutputSingleFile(outputFile, dict);
            else
                ProcessOutputMultipleFile(outputFile, dict);
        }
    }
}