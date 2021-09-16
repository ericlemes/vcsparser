using System;
using System.Collections.Generic;
using vcsparser.core.Database.Cosmos;

namespace vcsparser.core
{
    public interface ICosmosDbOutputProcessor
    {
        Dictionary<DateTime, Dictionary<string, T>> GetDocumentsByDateRange<T>(DateTime fromDateTime,
            DateTime endDateTime) where T : IOutputJson;
    }
}
