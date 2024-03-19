using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Factory;
using Xunit;

namespace vcsparser.unittests.Database.Cosmos
{
    /*public class GivenACosmosConnection
    {
        private readonly string someDatabaseId = "SomeDatabaseId";
        private readonly string someCollectionId = "SomeCollectionId";
        private readonly string someDocumentId = "SomeDocumentId";
        private readonly string someDocumentData = "Some Document Data";
        private readonly string someDocumentETag = "Some Document ETag";
        private readonly string someSelfLink = "Some Self Link";

        private readonly int someBatchBulkSize = 600;

        private Uri someCollectionUri;
        private Uri someDocumentUri;
        private Uri someDatabaseUri;
        private Document someDocument;
        private RequestOptions someRequestOptions;
        private FeedOptions someFeedOptions;

        private Mock<IDatabaseFactory> databaseFactory;
        private Mock<IDocumentClient> documentClient;
        private Mock<IBulkExecutorWrapper> bulkExecutorWrapper;
        private ConnectionPolicy connectionPolicy;
        private Mock<RetryOptions> retryOptions;
        private CosmosConnection sut;

        private DummyCosmosDocument someDummyDocument;
        private JsonSerializerSettings jsonSerializerSettings;
        private DocumentCollection someDocumentCollection;
        private BulkImportResponse someBulkImportResponse;

        public GivenACosmosConnection()
        {
            someDummyDocument = new DummyCosmosDocument
            {
                Id = someDocumentId,
                Data = someDocumentData,
                ETag = someDocumentETag,
                SelfLink = someSelfLink
            };

            jsonSerializerSettings = new JsonSerializerSettings();
            someRequestOptions = new RequestOptions();
            someFeedOptions = new FeedOptions();

            someDocument = new Document();
            someDocument.LoadFrom(new JsonTextReader(new StringReader(JsonConvert.SerializeObject(someDummyDocument))), jsonSerializerSettings);

            someCollectionUri = UriFactory.CreateDocumentCollectionUri(someDatabaseId, someCollectionId);
            someDocumentUri = UriFactory.CreateDocumentUri(someDatabaseId, someCollectionId, someDocumentId);

            connectionPolicy = new ConnectionPolicy();
            retryOptions = new Mock<RetryOptions>();
            retryOptions.Object.MaxRetryAttemptsOnThrottledRequests = 9;
            retryOptions.Object.MaxRetryWaitTimeInSeconds = 30;
            connectionPolicy.RetryOptions = retryOptions.Object;

            documentClient = new Mock<IDocumentClient>();
            documentClient.Setup(x =>  x.ConnectionPolicy).Returns(connectionPolicy);
            documentClient.Setup(x => x.CreateDocumentAsync(someCollectionUri, someDummyDocument, It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new ResourceResponse<Document>(someDocument)));

            documentClient.Setup( x=>x.CreateDocumentQuery<DummyCosmosDocument>(someCollectionUri, It.IsAny<FeedOptions>()))
                .Returns(new List<DummyCosmosDocument> { someDummyDocument }.AsQueryable().OrderBy(d => d.Id));

            documentClient.Setup(x=>x.CreateDocumentQuery<DummyCosmosDocument>(someCollectionUri, It.IsAny<SqlQuerySpec>(), It.IsAny<FeedOptions>()))
                .Returns(new List<DummyCosmosDocument> { someDummyDocument }.AsQueryable().OrderBy(d => d.Id));

            databaseFactory = new Mock<IDatabaseFactory>();
            databaseFactory.Setup(x => x.DocumentClient()).Returns(documentClient.Object);

            someDocumentCollection = new DocumentCollection
            {
                Id = someCollectionId
            };

            bulkExecutor = new Mock<IBulkExecutor>();
            bulkExecutorWrapper = new Mock<IBulkExecutorWrapper>();
            someBulkImportResponse = new BulkImportResponse();

            bulkExecutor
                .Setup(b => b.BulkImportAsync(It.Is<IEnumerable<CosmosDocumentBase>>(l => l.Count() == 1), true, false, null, null, default))
                .Callback(() =>
                {
                    someBulkImportResponse.GetType().GetProperty("NumberOfDocumentsImported").SetValue(someBulkImportResponse, 1);
                    someBulkImportResponse.GetType().GetProperty("TotalRequestUnitsConsumed").SetValue(someBulkImportResponse, 1);
                    someBulkImportResponse.GetType().GetProperty("TotalTimeTaken").SetValue(someBulkImportResponse, TimeSpan.FromSeconds(10));
                })
                .Returns(Task.FromResult(someBulkImportResponse));

            bulkExecutor
                .Setup(b => b.BulkImportAsync(It.Is<IEnumerable<CosmosDocumentBase>>(l => l.Count() == 2), true, false, null, null, default))
                .Callback(() =>
                {
                    someBulkImportResponse.GetType().GetProperty("NumberOfDocumentsImported").SetValue(someBulkImportResponse, 2);
                    someBulkImportResponse.GetType().GetProperty("TotalRequestUnitsConsumed").SetValue(someBulkImportResponse, 2);
                    someBulkImportResponse.GetType().GetProperty("TotalTimeTaken").SetValue(someBulkImportResponse, TimeSpan.FromSeconds(20));
                })
                .Returns(Task.FromResult(someBulkImportResponse));

            someDatabaseUri = UriFactory.CreateDatabaseUri(someDatabaseId);

            documentClient.Setup(x => x.CreateDocumentCollectionQuery(someDatabaseUri, It.IsAny<FeedOptions>()))
                .Returns(new List<DocumentCollection> { someDocumentCollection }.AsQueryable().OrderBy(x => x.PartitionKey));
            databaseFactory.Setup(x => x.DocumentClient()).Returns(documentClient.Object);
            databaseFactory.Setup(x => x.BulkExecutor(documentClient.Object, someDocumentCollection)).Returns(bulkExecutor.Object);
            databaseFactory.Setup(x => x.BulkExecutorWrapper(bulkExecutor.Object)).Returns(bulkExecutorWrapper.Object);

            sut = new CosmosConnection(databaseFactory.Object, someDatabaseId, someBatchBulkSize);
        }

        [Fact]
        public void WhenInitializeShouldCreateDocumentClient()
        {
            databaseFactory.Verify(x =>  x.DocumentClient(), Times.Once);
        }

        [Fact]
        public void WhenCreateDocumentAndOptionsNullShouldCreateOptionsAndReturnResponse()
        {
            var result = sut.CreateDocument(someCollectionId, someDummyDocument).Result;

            documentClient.Verify(x =>x.CreateDocumentAsync(someCollectionUri, someDummyDocument, It.IsAny<RequestOptions>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
            
            Assert.Equal(someDocument, result);
        }

        [Fact]
        public async void WhenCreateDocumentShouldReturnResponse()
        {
            var result = await sut.CreateDocument(someCollectionId, someDummyDocument, someRequestOptions);

            documentClient.Verify(x=>x.CreateDocumentAsync(someCollectionUri, someDummyDocument, someRequestOptions, It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);

            Assert.Equal(someDocument, result);
        }


        [Fact]
        public void WhenCreateDocumentQueryAndOptionsNullShouldCreateOptionsAndReturnOrderedQueryable()
        {
            var query = new SqlQuerySpec();

            var result = sut.CreateDocumentQuery<DummyCosmosDocument>(someCollectionId, query);

            documentClient.Verify(x=> x.CreateDocumentQuery<DummyCosmosDocument>(someCollectionUri, query, It.Is<FeedOptions>(o =>
                o.MaxItemCount == -1 &&
                o.EnableCrossPartitionQuery == true )), Times.Once);
            Assert.Equal(result.Single(), someDummyDocument);
        }

        [Fact]
        public void WhenCreateDocumentQueryShouldReturnOrderedQueryable()
        {
            var query = new SqlQuerySpec();

            var result = sut.CreateDocumentQuery<DummyCosmosDocument>(someCollectionId, query, someFeedOptions);

            documentClient.Verify(x=> x.CreateDocumentQuery<DummyCosmosDocument>(someCollectionUri, query, someFeedOptions), Times.Once);

            Assert.Equal(result.Single(), someDummyDocument);
        }


        [Fact]
        public void WhenDeleteDocumentByDocumentIdAndOptionsNullShouldDeleteDocumentAsync()
        {
            sut.DeleteDocument(someCollectionId, someDocumentId).Wait();

            documentClient.Verify(x =>x.DeleteDocumentAsync(someDocumentUri, It.Is<RequestOptions>(o =>
                o.PartitionKey.Equals(new PartitionKey(someDocumentId))
            ),It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenDeleteDocumentByDocumentIdShouldDeleteDocumentAsync()
        {
            sut.DeleteDocument(someCollectionId, someDocumentId, someRequestOptions).Wait();

            documentClient.Verify(x=> x.DeleteDocumentAsync(someDocumentUri, someRequestOptions, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public void WhenBulkInsertBatchesShouldRunCorrectCalls()
        {
            var documentBatch = new List<CosmosDocumentBase> { someDummyDocument };
            var documentBatches = new List<List<CosmosDocumentBase>> { documentBatch };

            var result = sut.BulkInsertBatches(someCollectionId, documentBatches, null).Result;

            documentClient.Verify(x => x.CreateDocumentCollectionQuery(someDatabaseUri, It.IsAny<FeedOptions>()), Times.Exactly(1));
            databaseFactory.Verify(x=> x.BulkExecutor(documentClient.Object, someDocumentCollection), Times.Exactly(1));
            bulkExecutor.Verify(x => x.InitializeAsync(), Times.Exactly(1));
            bulkExecutor.Verify(x => x.BulkImportAsync(documentBatch,
                true,false, null, null, default), Times.Exactly(1));

            Assert.NotNull(result);
            Assert.Equal(30, retryOptions.Object.MaxRetryWaitTimeInSeconds);
            Assert.Equal(9, retryOptions.Object.MaxRetryAttemptsOnThrottledRequests);
        }

        [Fact]
        public void WhenBulkInsertBatchesWithActionShouldCallAction()
        {
            var documentBatch = new List<CosmosDocumentBase> { someDummyDocument };
            var documentBatches = new List<List<CosmosDocumentBase>> { documentBatch };
            var batchAction = new Mock<Action<CosmosBulkImportSummary>>();

            var result = sut.BulkInsertBatches(someCollectionId, documentBatches, batchAction.Object).Result;

            batchAction.Verify(x => x.Invoke(It.IsAny<CosmosBulkImportSummary>()), Times.Exactly(1));
            Assert.NotNull(result);
        }

        [Fact]
        public void WhenBulkDeleteDocumentsShouldCallBulkDelete()
        {
            var list = new List<Tuple<string, string>>();
            list.Add(new Tuple<string, string>("some partition key", "some document id"));
            list.Add(new Tuple<string, string>("some other partition key", "some other document id"));

            var result = sut.BulkDeleteDocuments(someCollectionId, list);

            documentClient.Verify(x => x.CreateDocumentCollectionQuery(someDatabaseUri, It.IsAny<FeedOptions>()), Times.Exactly(1));
            databaseFactory.Verify(x => x.BulkExecutor(documentClient.Object, someDocumentCollection), Times.Exactly(1));
            bulkExecutor.Verify(x => x.InitializeAsync(), Times.Exactly(1));

            databaseFactory.Verify(x => x.BulkExecutorWrapper(bulkExecutor.Object), Times.Exactly(1));
            bulkExecutorWrapper.Verify(x => x.BulkDeleteAsync(list, someBatchBulkSize, default), Times.Exactly(1));

            Assert.NotNull(result);

            Assert.Equal(30, retryOptions.Object.MaxRetryWaitTimeInSeconds);
            Assert.Equal(9, retryOptions.Object.MaxRetryAttemptsOnThrottledRequests);
        }

        public static IEnumerable<object[]> WhenBulkInsertByBatchSizeShouldCreateBatchesData(int numTests)
        {
            var testDocument = new CosmosDocumentBase();
            var allData = new List<object[]>
            {
                new object[] {2, new CosmosDocumentBase[] {testDocument}, 1, 1},
                new object[] {1, new CosmosDocumentBase[] {testDocument, testDocument}, 2, 2},
                new object[] {2, new CosmosDocumentBase[] {testDocument, testDocument}, 2, 1},
                new object[] {2, new CosmosDocumentBase[] {testDocument, testDocument, testDocument}, 3, 2}
            };

            return allData.Take(numTests);
        }

        [Theory]
        [MemberData(nameof(WhenBulkInsertByBatchSizeShouldCreateBatchesData), parameters:4)]
        public void WhenBulkInsertByBatchSizeShouldCreateBatches(int numberOfDocumentsPerBatch, CosmosDocumentBase[] documents, int numberOfDocumentsInserted, int numberOfBatches)
        {
            sut = new CosmosConnection(databaseFactory.Object, someDatabaseId, numberOfDocumentsPerBatch);

            var result = sut.BulkInsertByBatchSize(someCollectionId, documents).Result;

            Assert.Equal(numberOfDocumentsInserted, result.NumberOfDocumentsInserted);
            Assert.Equal(numberOfBatches, result.NumberOfBatches);
        }

        [Fact]
        public void WhenCreateDocumentCollectionQueryAndHasCollectionShouldReturnDocumentCollection()
        {
            var result = sut.CreateDocumentCollectionQuery(someCollectionId, someFeedOptions);

            documentClient.Verify(x => x.CreateDocumentCollectionQuery(someDatabaseUri, someFeedOptions), Times.Exactly(1));
            Assert.Equal(result, someDocumentCollection);
        }
    }*/
}
