using System;
using System.Collections.Generic;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public abstract class OutputProcessor : IOutputProcessor
    {
        public abstract void ProcessOutput<T>(OutputType outputType, string outputFile,
            Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;


        protected SortedList<DateTime, SortedList<DailyCodeChurn, DailyCodeChurn>> ConvertCodeChurnDictToOrderedListPerDay(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
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

        protected IList<DailyCodeChurn> ConvertCodeChurnDictToOrderedList(Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> dict)
        {
            SortedList<DailyCodeChurn, DailyCodeChurn> sortedList = new SortedList<DailyCodeChurn, DailyCodeChurn>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (b.Value.TotalLinesChanged > 0)
                        sortedList.Add(b.Value, b.Value);
            return sortedList.Values;
        }

        protected SortedList<DateTime, SortedList<WorkItem, WorkItem>> ConvertBugDatabaseDictToOrderedListPerDay(Dictionary<DateTime, Dictionary<string, WorkItem>> dict)
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

        protected IList<WorkItem> ConvertBugDatabaseDictToOrderedList(Dictionary<DateTime, Dictionary<string, WorkItem>> dict)
        {
            SortedList<WorkItem, WorkItem> sortedList = new SortedList<WorkItem, WorkItem>();
            foreach (var a in dict)
                foreach (var b in a.Value)
                    if (!string.IsNullOrWhiteSpace(b.Value.ChangesetId))
                        sortedList.Add(b.Value, b.Value);
            return sortedList.Values;
        }

        protected IList<T> ConvertDictToOrderedList<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (typeof(T) == typeof(DailyCodeChurn))
                return ConvertCodeChurnDictToOrderedList(dict as Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>) as IList<T>;
            if (typeof(T) == typeof(WorkItem))
                return ConvertBugDatabaseDictToOrderedList(dict as Dictionary<DateTime, Dictionary<string, WorkItem>>) as IList<T>;
            return new List<T>();
        }

        protected SortedList<DateTime, SortedList<T, T>> ConvertDictToOrderedListPerDay<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson
        {
            if (typeof(T) == typeof(DailyCodeChurn))
                return ConvertCodeChurnDictToOrderedListPerDay(dict as Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>) as SortedList<DateTime, SortedList<T, T>>;
            if (typeof(T) == typeof(WorkItem))
                return ConvertBugDatabaseDictToOrderedListPerDay(dict as Dictionary<DateTime, Dictionary<string, WorkItem>>) as SortedList<DateTime, SortedList<T, T>>;
            return new SortedList<DateTime, SortedList<T, T>>();
        }
    }
}
