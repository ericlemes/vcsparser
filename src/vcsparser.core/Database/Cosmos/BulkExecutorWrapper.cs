using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace vcsparser.core.Database.Cosmos
{
    [ExcludeFromCodeCoverage]
    public class BulkExecutorWrapper //: IBulkExecutorWrapper
    {
        /*private readonly BulkExecutor bulkExecutor;

        public BulkExecutorWrapper(BulkExecutor bulkExecutor)
        {
            this.bulkExecutor = bulkExecutor;
        }

        // Excluding from CodeCoverage here because this function persists in showing no matter if the file is being ignored
        public async Task<BulkDeleteResponse> BulkDeleteAsync(List<Tuple<string, string>> pkIdTuplesToDelete, int? deleteBatchSize = null, CancellationToken cancellationToken = default)
        {
            return await bulkExecutor.BulkDeleteAsync(pkIdTuplesToDelete, deleteBatchSize, cancellationToken);
        }*/
    }
}
