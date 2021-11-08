using System;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosBulkImportSummary
    {
        public int NumberOfBatches { get; set; }
        public int NumberOfBatchesCompleted { get; set; }
        public long NumberOfDocumentsInserted { get; set; }
        public double TotalRequestUnitsConsumed { get; set; }
        public TimeSpan TotalTimeTaken { get; set; }
    }
}
