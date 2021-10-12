using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface IDataConverter
    {
        SortedList<DateTime, SortedList<T, T>> ConvertDictToOrderedListPerDay<T>(
            Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;

        IList<T> ConvertDictToOrderedList<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;
    }
}
