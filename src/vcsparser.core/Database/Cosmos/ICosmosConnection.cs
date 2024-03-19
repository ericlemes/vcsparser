using Microsoft.Azure.Documents;
using System.Threading.Tasks;
using Microsoft.Azure.Documents.Client;
using System.Linq;
using System.Collections.Generic;
using System;
using Microsoft.Azure.Cosmos;

namespace vcsparser.core.Database.Cosmos
{
    public interface ICosmosConnection
    {
        Task<T> CreateItem<T>(string containerId, T item) where T : CosmosDocumentBase;

        Task DeleteItem<T>(string containerId, string partitionKey, T item) where T : CosmosDocumentBase;
        Task DeleteItems<T>(string containerId, string partitionKey, IEnumerable<T> items) where T : CosmosDocumentBase;
        Task<List<T>> QueryItems<T>(string containerId, QueryDefinition queryDefinition) where T : CosmosDocumentBase;
    }
}
