﻿using Microsoft.Azure.CosmosDB.BulkExecutor;
using Microsoft.Azure.Documents.Client;
using Moq;
using Newtonsoft.Json;
using System;
using Microsoft.Azure.Documents;
using vcsparser.core;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Factory;
using Xunit;

namespace vcsparser.unittests.Factory
{
    public class GivenADatabaseFactory
    {
        private readonly string cosmosEndpoint = "https://somedatabase:443";
        private readonly string cosmosDBKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private readonly int someBatchBulkSize = 600;
        private readonly DownloadFromCosmosDbCommandLineArgs args;

       private readonly DatabaseFactory sut;

        public GivenADatabaseFactory()
        {
            args = new DownloadFromCosmosDbCommandLineArgs
            {
                CosmosEndpoint = cosmosEndpoint,
                CosmosDbKey = cosmosDBKey
            };
            sut = new DatabaseFactory(args, new JsonSerializerSettings());
        }

        [Fact]
        public void WhenCreatingDocumentClientShouldAllPropertiesShouldHaveCorrectValues()
        {
            var documentClient = sut.DocumentClient();
            Assert.Equal(ConnectionMode.Direct, documentClient.ConnectionPolicy.ConnectionMode);
            Assert.Equal(Protocol.Tcp, documentClient.ConnectionPolicy.ConnectionProtocol);
            Assert.Equal(50, documentClient.ConnectionPolicy.MaxConnectionLimit);
            Assert.Equal(30, documentClient.ConnectionPolicy.RetryOptions.MaxRetryWaitTimeInSeconds);
            Assert.Equal(9, documentClient.ConnectionPolicy.RetryOptions.MaxRetryAttemptsOnThrottledRequests);
            Assert.Contains("https://somedatabase", documentClient.ServiceEndpoint.AbsoluteUri);
        }


        [Fact]
        public void WhenCosmosConnectionShouldNotThrowExceptions()
        {
            var result = Record.Exception(() =>
            {
                sut.CosmosConnection("some-id", someBatchBulkSize);
            });
            Assert.Null(result);
        }

        [Fact]
        public void WhenCreatingDatabaseFactoryWithoutCosmosDbKeyShouldThrowArgumentNullException()
        {
            var commandLineArgs = new DownloadFromCosmosDbCommandLineArgs();

            Action action = () =>
            {
                 new DatabaseFactory(commandLineArgs, new JsonSerializerSettings());
            };

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null.\r\nParameter name: CosmosDbKey", exception.Message);
        }

        [Fact]
        public void WhenCreatingDatabaseFactoryWithoutCosmosEndpointShouldThrowArgumentNullException()
        {
            var commandLineArgs = new DownloadFromCosmosDbCommandLineArgs
            {
                CosmosDbKey = cosmosDBKey
            };

            Action action = () =>
            {
                new DatabaseFactory(commandLineArgs, new JsonSerializerSettings());
            };

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null.\r\nParameter name: CosmosEndpoint", exception.Message);
        }

        [Fact]
        public void WhenCreatingDatabaseFactoryWithGitExtractToCosmosDbCommandLineArgsAndNoEndpointShouldThrowArgumentNullException()
        {
            var commandLineArgs = new GitExtractToCosmosDbCommandLineArgs
            {
                CosmosDbKey = cosmosDBKey
            };

            Action action = () =>
            {
                new DatabaseFactory(commandLineArgs, new JsonSerializerSettings());
            };

            var exception = Assert.Throws<ArgumentNullException>(action);
            Assert.Equal("Value cannot be null.\r\nParameter name: CosmosEndpoint", exception.Message);
        }


        [Fact]
        public void WhenCreatingDatabaseFactoryWithGitExtractToCosmosDbCommandLineArgsAndWithProperParametersShouldCreateInstance()
        {
            var commandLineArgs = new GitExtractToCosmosDbCommandLineArgs
            {
                CosmosDbKey = cosmosDBKey,
                CosmosEndpoint = cosmosEndpoint
            };

            var dbFactory = new DatabaseFactory(commandLineArgs, new JsonSerializerSettings());

            Assert.NotNull(dbFactory);
        }


        [Fact]
        public void WhenBulkExecutorWrapperShouldDoesNotThrowExceptions()
        {
            var exception = Record.Exception(() => sut.BulkExecutorWrapper(new Mock<IBulkExecutor>().Object));

            Assert.Null(exception);
        }

        [Fact]
        public void WhenBulkExecutorShouldDoesNotThrowExceptions()
        {
            var exception = Record.Exception(() => sut.BulkExecutor(sut.DocumentClient(), new Mock<DocumentCollection>().Object));

            Assert.Null(exception);
        }
    }
}
