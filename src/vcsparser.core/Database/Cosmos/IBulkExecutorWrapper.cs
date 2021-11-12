using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;

namespace vcsparser.core.Database.Cosmos
{
    public interface IBulkExecutorWrapper
    {
        Task<BulkDeleteResponse> BulkDeleteAsync(List<Tuple<string, string>> pkIdTuplesToDelete, int? deleteBatchSize = null, CancellationToken cancellationToken = default);
    }
}
