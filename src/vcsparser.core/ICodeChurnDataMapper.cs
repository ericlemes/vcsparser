using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public interface ICodeChurnDataMapper
    {
        SortedList<DateTime, SortedList<T, T>> ConvertDictToOrderedListPerDay<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;

        IList<T> ConvertDictToOrderedList<T>(Dictionary<DateTime, Dictionary<string, T>> dict) where T : IOutputJson;
    }
}
