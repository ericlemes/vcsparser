using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Moq;
using Moq.Language.Flow;
using Newtonsoft.Json;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Factory;
using Xunit;

namespace vcsparser.unittests.Database.Cosmos
{
    public class GivenACosmosConnection
    {
        private readonly string someDatabaseId = "SomeDatabaseId";
        private readonly string someCollectionId = "SomeCollectionId";
        private readonly string someDocumentId = "SomeDocumentId";
        private readonly string someDocumentData = "Some Document Data";
        private readonly string someDocumentETag = "Some Document ETag";
        private readonly string someSelfLink = "Some Self Link";

        private Uri someCollectionUri;
        private Uri someDocumentUri;
        private Document someDocument;
        private RequestOptions someRequestOptions;
        private FeedOptions someFeedOptions;

        private Mock<IDatabaseFactory> databaseFactory;
        private Mock<IDocumentClient> documentClient;
        private ConnectionPolicy connectionPolicy;
        private RetryOptions retryOptions;
        private CosmosConnection sut;

        private DummyCosmosDocument someDummyDocument;
        private JsonSerializerSettings jsonSerializerSettings;

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
            retryOptions = new RetryOptions
            {
                MaxRetryWaitTimeInSeconds = 30,
                MaxRetryAttemptsOnThrottledRequests = 9
            };
            connectionPolicy.RetryOptions = retryOptions;

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

            sut = new CosmosConnection(databaseFactory.Object, someDatabaseId);
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
    }
}
