using System;
using System.Collections.Generic;

namespace vcsparser.core
{
    public interface ICosmosDbOutputProcessor
    {
        Dictionary<DateTime, Dictionary<string, DailyCodeChurn>> GetDocumentsBasedOnDateRange(DateTime fromDateTime,
            DateTime endDateTime);
    }
}
