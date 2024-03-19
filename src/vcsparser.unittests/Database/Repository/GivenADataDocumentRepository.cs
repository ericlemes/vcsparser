using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Moq;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database.Cosmos;
//using vcsparser.core.Database.Repository;
using Xunit;

namespace vcsparser.unittests.Database.Repository
{
    /*public class GivenADataDocumentRepository
    {
        private readonly string someProjectName = "some-project-name";
        private readonly string someFileName = "some-file-name";
        private readonly string cosmosContainer = "some-cosmos-container";
        private readonly DataDocumentRepository sut;
        private readonly Mock<ICosmosConnection> cosmosConnectionMock;

        public GivenADataDocumentRepository()
        {
            cosmosConnectionMock = new Mock<ICosmosConnection>();

            sut = new DataDocumentRepository(cosmosConnectionMock.Object, cosmosContainer);
        }

        [Fact]
        public void WhenCreateDataDocumentShouldCallCreateDocumentFromCosmosConnection()
        {
            var cosmosDocument = new CosmosDataDocument<WorkItem>();

            sut.CreateDataDocument(cosmosDocument);

            cosmosConnectionMock.Verify(x => x.CreateDocument(cosmosContainer, cosmosDocument, null), Times.Once);
        }


        [Fact]
        public void WhenDeleteMultipleDocumentsShouldCallCosmosDbWithCorrectQueries()
        {
            var closedDate1 = DateTime.Now.AddDays(-1);
            var closedDate2 = DateTime.Now.AddDays(-2);

            var workItems1 = new List<WorkItem>
            {
                new WorkItem
                {
                    WorkItemId = "Id1",
                    ClosedDate = closedDate1,
                    ChangesetId = "some-changeset-id-1"
                }
            };

            var workItems2 = new List<WorkItem>
            {
                new WorkItem
                {
                    WorkItemId = "Id2",
                    ClosedDate = closedDate2,
                    ChangesetId = "some-changeset-id-2"
                }
            };

            var documentsToDeleteToReturn1 = new List<CosmosDataDocument<WorkItem>>
            {
                new CosmosDataDocument<WorkItem>
                {
                    Data = workItems1,
                    DocumentType = DocumentType.BugDatabase,
                    ProjectName = "document-name1",
                    DateTime = closedDate1.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };

            var documentsToReturn2 = new List<CosmosDataDocument<WorkItem>>
            {
                new CosmosDataDocument<WorkItem>
                {
                    Data = workItems2,
                    DocumentType = DocumentType.BugDatabase,
                    ProjectName = "document-name2",
                    DateTime = closedDate2.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };

            var documentsToDelete = new List<CosmosDataDocument<WorkItem>>
            {
                new CosmosDataDocument<WorkItem>
                {
                    Data = workItems1,
                    DocumentType = DocumentType.BugDatabase,
                    ProjectName = "document-name1",
                    DateTime = closedDate1.ToString("yyyy-MM-ddTHH:mm:ss")
                },

                new CosmosDataDocument<WorkItem>
                {
                    Data = workItems2,
                    DocumentType = DocumentType.BugDatabase,
                    ProjectName = "document-name2",
                    DateTime = closedDate2.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };

            var sqlQuery1 =
                $"SELECT * FROM c WHERE c.documentType= '{DocumentType.BugDatabase}' and c.projectName = 'document-name1' and c.occurrenceDate = '{closedDate1:yyyy-MM-ddTHH:mm:ss}'";

            var sqlQuery2 =
                $"SELECT * FROM c WHERE c.documentType= '{DocumentType.BugDatabase}' and c.projectName = 'document-name2' and c.occurrenceDate = '{closedDate2:yyyy-MM-ddTHH:mm:ss}'";

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery1), null))
                .Returns(documentsToDeleteToReturn1.AsEnumerable().AsQueryable());

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery2), null))
                .Returns(documentsToReturn2.AsEnumerable().AsQueryable());

            sut.DeleteMultipleDocuments(documentsToDelete);

            cosmosConnectionMock.Verify(x => x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery1), null), Times.Exactly(1)); 
            cosmosConnectionMock.Verify(x => x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery2), null), Times.Exactly(1));
            cosmosConnectionMock.Verify(x => x.DeleteDocument(cosmosContainer, It.IsAny<string>(), null), Times.Exactly(2));
        }

        [Fact]
        public void WhenDeleteMultipleDocumentsWithNoExisringDbItemsShouldNeverCallCosmosDb()
        {
            var closedDate = DateTime.Now.AddDays(-1);

            var items = new List<WorkItem>
            {
                new WorkItem
                {
                    WorkItemId = "Id1",
                    ClosedDate = closedDate,
                    ChangesetId = "some-changeset-id-1"
                }
            };

            var documentsToDelete = new List<CosmosDataDocument<WorkItem>>
            {
                new CosmosDataDocument<WorkItem>
                {
                    Data = items,
                    DocumentType = DocumentType.BugDatabase,
                    ProjectName = "document-name1",
                    DateTime = closedDate.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };
            var sqlQuery =
                $"SELECT * FROM c WHERE c.documentType= '{DocumentType.BugDatabase}' and c.documentName = 'document-name2' and c.occurrenceDate = '{closedDate:yyyy-MM-ddTHH:mm:ss}'";

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null))
                .Returns(new List<CosmosDataDocument<WorkItem>>().AsEnumerable().AsQueryable());

            sut.DeleteMultipleDocuments(documentsToDelete);

            cosmosConnectionMock.Verify(x => x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null), Times.Never);
            cosmosConnectionMock.Verify(x => x.DeleteDocument(cosmosContainer, It.IsAny<string>(), null), Times.Never);
        }

        [Fact]
        public void WhenGetDocumentsInDateRangeReturnsValuesShouldReturnExpectedValues()
        {
            var startDate = DateTime.Now.AddDays(-2); ;
            var endDate = DateTime.Now;
            var sqlQuery = new SqlQuerySpec($"SELECT * FROM c WHERE c.projectName = '{someProjectName}' and c.documentType = '{DocumentType.BugDatabase}' and (c.occurrenceDate between '{ startDate.ToString(CosmosDataDocument<WorkItem>.DATE_FORMAT) }' and '{ endDate.ToString(CosmosDataDocument<WorkItem>.DATE_FORMAT) }') order by c.occurrenceDate desc");
            
            var toReturn = new List<CosmosDataDocument<WorkItem>>
            {
                
                new CosmosDataDocument<WorkItem>
                {
                    Data = new List<WorkItem>
                    {
                        new WorkItem
                        {
                            WorkItemId = "Id1",
                            ClosedDate = endDate,
                            ChangesetId = "some-changeset-id-1"
                        }
                    }
                }
            };

            cosmosConnectionMock.Setup(x => x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery.QueryText), null))
                .Returns(toReturn.AsQueryable());

            var result = sut.GetDocumentsInDateRange<WorkItem>(someProjectName, DocumentType.BugDatabase, startDate, endDate);

            cosmosConnectionMock.Verify(x => x.CreateDocumentQuery<CosmosDataDocument<WorkItem>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery.QueryText), null), Times.Once);

            Assert.Single(result);
            var document = toReturn[0].Data[0];
            var toCompare = toReturn[0].Data[0];

            Assert.True(document.ChangesetId == toCompare.ChangesetId);
            Assert.True(document.WorkItemId == toCompare.WorkItemId);
            Assert.True(document.ClosedDate == toCompare.ClosedDate);
        }

        [Fact]
        public void WhenBatchInsertCosmosDocumentsShouldCallBulkInsertByBatchSizeFromCosmosConnection()
        {
            var data = new List<CosmosDataDocument<DailyCodeChurn>>
            {
                new CosmosDataDocument<DailyCodeChurn>
                {
                    Data = new List<DailyCodeChurn>
                    {
                        new DailyCodeChurn
                        {
                            FileName = someFileName
                        }
                    }
                }
            };

            sut.BatchInsertCosmosDocuments(data, x => { });

            cosmosConnectionMock.Verify(x => x.BulkInsertByBatchSize(cosmosContainer, data, It.IsAny<Action<CosmosBulkImportSummary>>()), Times.Once);
        }

        [Fact]
        public void WhenBatchDeleteDocumentsShouldMakeAProperQueryAndCallBulkDeleteDocumentsFromCosmosConnection()
        {
            var documentType = DocumentType.CodeChurn;
            var startDate = DateTime.Now.AddDays(-1);
            var endDate = DateTime.Now;
            var documentsToDeleteToReturn = new List<CosmosDataDocument<DailyCodeChurn>>
            {
                new CosmosDataDocument<DailyCodeChurn>
                {
                    DateTime = startDate.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };

            var sqlQuery =  $"SELECT * FROM c WHERE c.documentType = '{documentType}' and c.projectName = '{someProjectName}' and (c.occurrenceDate between '{startDate.AddDays(-1):yyyy/MM/ddTHH:mm:ss}' and '{endDate:yyyy/MM/ddTHH:mm:ss}')";

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDocumentBase>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null))
                .Returns(documentsToDeleteToReturn.AsEnumerable().AsQueryable());

            sut.BatchDeleteDocuments(startDate, endDate, someProjectName, documentType);

            cosmosConnectionMock.Verify(x => x.BulkDeleteDocuments(cosmosContainer, It.IsAny<List<Tuple<string, string>>>()), Times.Once);
        }

        [Fact]
        public void WhenBatchDeleteDocumentsAndNoDocumentsFoundShouldNeverCallBulkDeleteCommentsFromCosmosConnection()
        {
            var documentType = DocumentType.CodeChurn;
            var startDate = DateTime.Now.AddDays(-1);
            var endDate = DateTime.Now;

            var sqlQuery = $"SELECT * FROM c WHERE c.documentType = '{documentType}' and c.projectName = '{someProjectName}' and (c.occurrenceDate between '{startDate.AddDays(-1):yyyy/MM/ddTHH:mm:ss}' and '{endDate:yyyy/MM/ddTHH:mm:ss}')";

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDocumentBase>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null))
                .Returns(new List<CosmosDataDocument<DailyCodeChurn>>().AsEnumerable().AsQueryable());

            sut.BatchDeleteDocuments(startDate, endDate, someProjectName, documentType);

            cosmosConnectionMock.Verify(x => x.BulkDeleteDocuments(cosmosContainer, It.IsAny<List<Tuple<string, string>>>()), Times.Never);
        }

        [Fact]
        public void WhenGetAllDocumentsByProjectAndDocumentTypeShouldReturnCorrectData()
        {
            var documentType = DocumentType.CodeChurn;
            var sqlQuery =
                $"SELECT * FROM c WHERE c.projectName = '{someProjectName}' and c.documentType = '{documentType}' order by c.occurrenceDate desc";

            cosmosConnectionMock.Setup(x =>
                    x.CreateDocumentQuery<CosmosDocumentBase>(It.IsAny<string>(), It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null))
                .Returns(new List<CosmosDataDocument<DailyCodeChurn>>().AsEnumerable().AsQueryable());

            sut.GetAllDocumentsByProjectAndDocumentType<DailyCodeChurn>(someProjectName, documentType);

            cosmosConnectionMock.Verify(x => x.CreateDocumentQuery<CosmosDataDocument<DailyCodeChurn>>(cosmosContainer, It.Is<SqlQuerySpec>(query => query.QueryText == sqlQuery), null), Times.Exactly(1));
        }
    }*/
}
