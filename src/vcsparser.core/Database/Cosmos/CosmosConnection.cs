using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using vcsparser.core.Factory;

namespace vcsparser.core.Database.Cosmos
{
    public class CosmosConnection : ICosmosConnection
    {
        private readonly string databaseId;
        private readonly int bulkBatchSize;
        private readonly int originalMaxRetryWaitTimeInSeconds;
        private readonly int originalMaxRetryAttemptsOnThrottledRequests;

        private readonly IDocumentClient documentClient;
        private readonly IDatabaseFactory databaseFactory;

        public CosmosConnection(IDatabaseFactory databaseFactory, string databaseId, int bulkBatchSize)
        {
            this.databaseFactory = databaseFactory;
            this.documentClient = databaseFactory.DocumentClient();
            this.databaseId = databaseId;
            this.bulkBatchSize = bulkBatchSize;

            this.originalMaxRetryWaitTimeInSeconds = documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds;
            this.originalMaxRetryAttemptsOnThrottledRequests = documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests;
        }

        [ExcludeFromCodeCoverage]
        //This method is ignored because OpenCover can't cover all branches for async methods
        public async Task<Document> CreateDocument(string collectionId, object document, RequestOptions options = null)
        {
            if (options == null)
                options = new RequestOptions { };

            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            var response = await documentClient.CreateDocumentAsync(collectionUri, document, options);
            return response.Resource;
        }

        [ExcludeFromCodeCoverage]
        //This method is ignored because OpenCover can't cover all branches for async methods
        public async Task DeleteDocument(string collectionId, string documentId, RequestOptions options = null)
        {
            if (options == null)
                options = new RequestOptions
                {
                    PartitionKey = new PartitionKey(documentId)
                };

            var documentUri = UriFactory.CreateDocumentUri(databaseId, collectionId, documentId);
            await documentClient.DeleteDocumentAsync(documentUri, options);
        }


        public IQueryable<T> CreateDocumentQuery<T>(string collectionId, SqlQuerySpec query, FeedOptions options = null)
        {
            if (options == null)
                options = new FeedOptions
                {
                    MaxItemCount = -1,
                    EnableCrossPartitionQuery = true
                };

            var collectionUri = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);
            return documentClient.CreateDocumentQuery<T>(collectionUri, query, options);
        }

        public async Task<CosmosBulkImportSummary> BulkInsertByBatchSize(string collectionId, IEnumerable<CosmosDocumentBase> documents, Action<CosmosBulkImportSummary> batchFinished = null)
        {
            var numberOfBatches = (int)Math.Ceiling(((double)documents.Count()) / bulkBatchSize);
            var documentBatches = CreateDocumentBatches(numberOfBatches, bulkBatchSize, documents);
            return await BulkInsertBatches(collectionId, documentBatches, batchFinished);
        }

        public async Task<CosmosBulkImportSummary> BulkInsertBatches(string collectionId, IEnumerable<IEnumerable<CosmosDocumentBase>> documentBatches, Action<CosmosBulkImportSummary> batchFinished = null)
        {
            try
            {
                var bulkExecutor = await GetAndInitializeBulkExecutor(collectionId);

                var jobSummray = new CosmosBulkImportSummary
                {
                    NumberOfBatches = documentBatches.Count()
                };

                foreach (var documentBatch in documentBatches)
                    await Task.Run(async () =>
                    {
                        var batchSummray = await BulkInsertAsync(bulkExecutor, documentBatch);

                        UpdateCosmosBulkImportSummary(jobSummray, batchSummray);
                        batchFinished?.Invoke(batchSummray);
                    });
                return jobSummray;
            }
            finally
            {
                PostBulkExecutor();
            }
        }

        private IEnumerable<IEnumerable<CosmosDocumentBase>> CreateDocumentBatches(int numberOfBatches, int numberOfDocumentsPerBatch, IEnumerable<CosmosDocumentBase> documents)
        {
            var documentBatches = new List<List<CosmosDocumentBase>>();
            for (var i = 0; i < numberOfBatches; i++)
            {
                var batchIndex = i * numberOfDocumentsPerBatch;
                var batch = new List<CosmosDocumentBase>();
                for (var j = 0; j < numberOfDocumentsPerBatch; j++)
                {
                    var documentIndex = batchIndex + j;
                    if (documentIndex >= documents.Count())
                        break;
                    batch.Add(documents.ElementAt(documentIndex));
                }
                documentBatches.Add(batch);
            }
            return documentBatches;
        }

        public DocumentCollection CreateDocumentCollectionQuery(string collectionId, FeedOptions options = null)
        {
            if (options == null)
                options = new FeedOptions
                {
                    MaxItemCount = 1
                };

            var databaseUri = UriFactory.CreateDatabaseUri(databaseId);

            return documentClient.CreateDocumentCollectionQuery(databaseUri, options)
                .Where(c => c.Id == collectionId).AsEnumerable().FirstOrDefault();
        }

        public async Task<BulkDeleteResponse> BulkDeleteDocuments(string collectionId, List<Tuple<string, string>> idsToDelete)
        {
            try
            {
                var bulkExecutor = await GetAndInitializeBulkExecutor(collectionId);
                var wrapper = this.databaseFactory.BulkExecutorWrapper(bulkExecutor);
                return await wrapper.BulkDeleteAsync(idsToDelete, bulkBatchSize);
            }
            finally
            {
                PostBulkExecutor();
            }
        }

        private async Task<IBulkExecutor> GetAndInitializeBulkExecutor(string collectionId)
        {
            var documentCollection = CreateDocumentCollectionQuery(collectionId);
            var bulkExecutor = databaseFactory.BulkExecutor(documentClient, documentCollection);
            await bulkExecutor.InitializeAsync();

            documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = 0;
            documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = 0;

            return bulkExecutor;
        }

        private void PostBulkExecutor()
        {
            documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds = originalMaxRetryWaitTimeInSeconds;
            documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests = originalMaxRetryAttemptsOnThrottledRequests;
        }

        private void UpdateCosmosBulkImportSummary(CosmosBulkImportSummary jobSummray, CosmosBulkImportSummary batchSummray)
        {
            jobSummray.NumberOfDocumentsInserted += batchSummray.NumberOfDocumentsInserted;
            jobSummray.TotalRequestUnitsConsumed += batchSummray.TotalRequestUnitsConsumed;
            jobSummray.TotalTimeTaken += batchSummray.TotalTimeTaken;
            jobSummray.NumberOfBatchesCompleted++;

            batchSummray.NumberOfBatchesCompleted = jobSummray.NumberOfBatchesCompleted;
            batchSummray.NumberOfBatches = jobSummray.NumberOfBatches;
        }

        private async Task<CosmosBulkImportSummary> BulkInsertAsync(IBulkExecutor bulkExecutor, IEnumerable<CosmosDocumentBase> documentBatch)
        {
            var bulkImportResponse = await bulkExecutor.BulkImportAsync(documentBatch,
                disableAutomaticIdGeneration: false,
                enableUpsert: true);

            return new CosmosBulkImportSummary
            {
                NumberOfDocumentsInserted = bulkImportResponse.NumberOfDocumentsImported,
                TotalRequestUnitsConsumed = bulkImportResponse.TotalRequestUnitsConsumed,
                TotalTimeTaken = bulkImportResponse.TotalTimeTaken
            };
        }
    }
}
