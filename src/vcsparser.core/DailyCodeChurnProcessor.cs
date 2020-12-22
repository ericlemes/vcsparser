using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.MeasureAggregators;

namespace vcsparser.core
{
    public class DailyCodeChurnProcessor
    {
        private IFileSystem fileSystem;

        private IJsonListParser<DailyCodeChurn> dailyCodeChurnParser;

        private ILogger logger;

        private IExclusionsProcessor exclusionsProcessor;
        private IInclusionsProcessor inclusionsProcessor;

        private FilePrefixRemover filePrefixRemover = new FilePrefixRemover();

        private IJsonExporter jsonExporter;

        public DailyCodeChurnProcessor(IFileSystem fileSystem, IJsonListParser<DailyCodeChurn> dailyCodeChurnParser, ILogger logger, IExclusionsProcessor exclusionsProcessor, IInclusionsProcessor inclusionsProcessor, IJsonExporter jsonExporter)
        {
            this.fileSystem = fileSystem;
            this.dailyCodeChurnParser = dailyCodeChurnParser;
            this.logger = logger;
            this.exclusionsProcessor = exclusionsProcessor;
            this.inclusionsProcessor = inclusionsProcessor;
            this.jsonExporter = jsonExporter;
        }

        public void Process(DailyCodeChurnCommandLineArgs a)
        {
            var dict = new SortedDictionary<DateTime, AggregatedDailyCodeChurn>();
            
            var files = fileSystem.GetFiles(a.InputDir, "*.json");

            foreach (var file in files)
            {
                this.logger.LogToConsole(String.Format("Processing {0}", file.FileName));
                var codeChurnList = this.dailyCodeChurnParser.ParseFile(file.FileName);

                foreach(var line in codeChurnList)
                {
                    ProcessLine(dict, line, filePrefixRemover.ProcessFileName(line.FileName, a.FilePrefixToRemove));
                }
            }

            var result = dict.ToList().Select(l => l.Value).ToList();
            this.jsonExporter.Export(result, a.OutputFile);
        }

        private void ProcessLine(SortedDictionary<DateTime, AggregatedDailyCodeChurn> dict, DailyCodeChurn line, string fileWithoutPrefix)
        {
            if (!this.inclusionsProcessor.IsIncluded(fileWithoutPrefix))
                return;

            if (this.exclusionsProcessor.IsExcluded(fileWithoutPrefix))
                return;

            DateTime key = line.GetDateTimeAsDateTime().Date;
            if (!dict.ContainsKey(key))
            {
                dict.Add(key, new AggregatedDailyCodeChurn()
                {
                    Timestamp = key.ToString(AggregatedDailyCodeChurn.DATE_FORMAT)
                });
            }

            AggregateLine(dict[key], line);
        }

        private void AggregateLine(AggregatedDailyCodeChurn aggregatedDailyCodeChurn, DailyCodeChurn line)
        {
            aggregatedDailyCodeChurn.Added += line.Added;
            aggregatedDailyCodeChurn.AddedInFixesVCS += line.AddedWithFixes;
            aggregatedDailyCodeChurn.ChangesAfter += line.ChangesAfter;
            aggregatedDailyCodeChurn.ChangesAfterInFixesVCS += line.ChangesAfterWithFixes;
            aggregatedDailyCodeChurn.ChangesBefore += line.ChangesBefore;
            aggregatedDailyCodeChurn.ChangesBeforeInFixesVCS += line.ChangesBeforeWithFixes;
            aggregatedDailyCodeChurn.Deleted += line.Deleted;
            aggregatedDailyCodeChurn.DeletedInFixesVCS += line.DeletedWithFixes;
            aggregatedDailyCodeChurn.NumberOfChanges += line.NumberOfChanges;
            aggregatedDailyCodeChurn.NumberOfChangesWithFixesVCS += line.NumberOfChangesWithFixes;

            AggregateAuthors(aggregatedDailyCodeChurn, line);
            AggregateBugDatabase(aggregatedDailyCodeChurn, line);
            aggregatedDailyCodeChurn.DailyCodeChurnPerFile.Add(line);
        }

        private static void AggregateBugDatabase(AggregatedDailyCodeChurn aggregatedDailyCodeChurn, DailyCodeChurn line)
        {
            if (line.BugDatabase == null)
                return;
            
            aggregatedDailyCodeChurn.AddedInFixesBugDB += line.BugDatabase.AddedInFixes;
            aggregatedDailyCodeChurn.ChangesAfterInFixesBugDB += line.BugDatabase.ChangesAfterInFixes;
            aggregatedDailyCodeChurn.ChangesBeforeInFixesBugDB += line.BugDatabase.ChangesBeforeInFixes;
            aggregatedDailyCodeChurn.DeletedInFixesBugDB += line.BugDatabase.DeletedInFixes;
            aggregatedDailyCodeChurn.NumberOfChangesWithFixesBugDB += line.BugDatabase.NumberOfChangesInFixes;            
        }

        private static void AggregateAuthors(AggregatedDailyCodeChurn aggregatedDailyCodeChurn, DailyCodeChurn line)
        {
            if (line.Authors == null)
                return;
            
            foreach (var a in line.Authors)
            {
                if (!aggregatedDailyCodeChurn.Authors.Exists(aut => aut.Author == a.Author))
                    aggregatedDailyCodeChurn.Authors.Add(new DailyCodeChurnAuthor()
                    {
                        Author = a.Author
                    });
                aggregatedDailyCodeChurn.Authors.Where(aut => aut.Author == a.Author).First().NumberOfChanges += a.NumberOfChanges;
            }            
        }
    }
}
