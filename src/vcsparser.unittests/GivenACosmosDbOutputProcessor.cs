using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Moq;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenACosmosDbOutputProcessor
    {
        private const string ProjectName = "some-project-name";
        private readonly CosmosDbOutputProcessor sut;
        private readonly int batchSize = 5;

        private readonly Mock<ILogger> loggerMock;
        private readonly Mock<IDataDocumentRepository> dataDocumentRepositoryMock;

        public GivenACosmosDbOutputProcessor()
        {
            loggerMock = new Mock<ILogger>();
            dataDocumentRepositoryMock = new Mock<IDataDocumentRepository>();
            sut = new CosmosDbOutputProcessor(loggerMock.Object, dataDocumentRepositoryMock.Object, new DataConverter(), ProjectName, batchSize);
        }

        [Fact]
        public void WhenProcessOutputShouldRunDatabaseLogicCorrectly()
        {
            var dateTime = new DateTime(2018, 08, 30);
            var fileName = "some-file-name";
            var codeChurn =
                new DailyCodeChurn()
                {
                    Added = 1,
                    ChangesBefore = 2,
                    ChangesAfter = 3,
                    Deleted = 4,
                    FileName = "abc",
                    Timestamp = "2018/08/30 00:00:00",
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 2
                        }
                    }
                };

            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { dateTime, new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } }
            };

            sut.ProcessOutput(OutputType.CosmosDb, string.Empty, dict);
            dataDocumentRepositoryMock.Verify(x => x.DeleteMultipleDocuments(It.Is<List<CosmosDataDocument<DailyCodeChurn>>>(items => 
                items.Any(y => CompareDailyCodeChurn(y.Data[0], codeChurn)))), Times.Once);

            dataDocumentRepositoryMock.Verify(x => x.CreateDataDocument(It.Is<CosmosDataDocument<DailyCodeChurn>>(
           y => CompareDailyCodeChurn(y.Data[0], codeChurn))));
        }

        [Fact]
        public void WhenGetDocumentsInDateRangeReturnsNullShouldReturnEmptyDictionary()
        {
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(1);

            var result = sut.GetDocumentsInDateRange<WorkItem>(startDate, endDate);

            dataDocumentRepositoryMock.Verify(x => x.GetDocumentsInDateRange<WorkItem>(ProjectName, DocumentType.BugDatabase, startDate, endDate), Times.Once);
            Assert.Empty(result);
        }

        [Fact]
        public void WhenGetDocumentsInDateRangeReturnsDataShouldReturnExpectedValues()
        {
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(1);
            var workItems = new List<WorkItem>
            {
                new WorkItem
                {
                    ChangesetId = "some-changset-id",
                    ClosedDate = startDate.AddMinutes(1),
                    WorkItemId = "some-workitem-id"
                }
            };
            var workItems2 = new List<WorkItem>
            {
                new WorkItem
                {
                    ChangesetId = "some-changset-id2",
                    ClosedDate = startDate.AddMinutes(2),
                    WorkItemId = "some-workitem-id2"
                }
            };
            var cosmosDataDocuments = new List<CosmosDataDocument<WorkItem>>
            {
                new CosmosDataDocument<WorkItem>
                { 
                    DateTime = startDate.ToString(DailyCodeChurn.DATE_FORMAT),
                    Data = workItems
                },
                new CosmosDataDocument<WorkItem>
                {
                    DateTime = startDate.ToString(DailyCodeChurn.DATE_FORMAT),
                    Data = workItems2
                },
            };
            dataDocumentRepositoryMock
                .Setup(x => x.GetDocumentsInDateRange<WorkItem>(ProjectName, DocumentType.BugDatabase, It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Returns(cosmosDataDocuments);

            var result = sut.GetDocumentsInDateRange<WorkItem>(startDate, endDate);

            dataDocumentRepositoryMock.Verify(x => x.GetDocumentsInDateRange<WorkItem>(ProjectName, DocumentType.BugDatabase, startDate, endDate), Times.Once);
            Assert.Single(result);
            var resultWorkItems = result.Values.ToList();
            Assert.True(CompareWorkitems(resultWorkItems[0].Values.ToList()[0], workItems[0]));
            Assert.True(CompareWorkitems(resultWorkItems[0].Values.ToList()[1], workItems2[0]));
        }

        [Fact]
        public void WhenProcessOutputWithMoreItemsThanBatchSizeShouldRunInsertBatchDocuments()
        {
            var dateTime = new DateTime(2018, 08, 30);
            var fileName = "some-file-name";
            var codeChurn =
                new DailyCodeChurn()
                {
                    Added = 1,
                    ChangesBefore = 2,
                    ChangesAfter = 3,
                    Deleted = 4,
                    FileName = "abc",
                    Timestamp = "2018/08/30 00:00:00",
                    Authors = new List<DailyCodeChurnAuthor>()
                    {
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author1",
                            NumberOfChanges = 1
                        },
                        new DailyCodeChurnAuthor()
                        {
                            Author = "author2",
                            NumberOfChanges = 2
                        }
                    }
                };

            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { dateTime, new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } },
                { dateTime.AddDays(1), new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } },
                { dateTime.AddDays(2), new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } },
                { dateTime.AddDays(3), new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } },
                { dateTime.AddDays(4), new Dictionary<string, DailyCodeChurn> { {fileName, codeChurn}  } }
            };

            var bulkDeleteResponse = new BulkDeleteResponse();
            var cosmosBulkImportSummary = new CosmosBulkImportSummary();

            dataDocumentRepositoryMock
                .Setup(x => x.BatchDeleteDocuments(dict.First().Key, dict.Last().Key, ProjectName, DocumentType.CodeChurn))
                .Returns(bulkDeleteResponse);

            dataDocumentRepositoryMock
                .Setup(x => x.BatchInsertCosmosDocuments(It.IsAny<List<CosmosDataDocument<DailyCodeChurn>>>(), It.IsAny<Action<CosmosBulkImportSummary>>()))
                .Returns(cosmosBulkImportSummary);

            sut.ProcessOutput(OutputType.CosmosDb, string.Empty, dict);
            dataDocumentRepositoryMock.Verify(x => x.DeleteMultipleDocuments(It.Is<List<CosmosDataDocument<DailyCodeChurn>>>(items =>
                items.Any(y => CompareDailyCodeChurn(y.Data[0], codeChurn)))), Times.Never);

            dataDocumentRepositoryMock.Verify(x => x.CreateDataDocument(It.Is<CosmosDataDocument<DailyCodeChurn>>(
                y => CompareDailyCodeChurn(y.Data[0], codeChurn))), Times.Never);

            dataDocumentRepositoryMock.Verify(x => x.BatchDeleteDocuments(dict.First().Key, dict.Last().Key, ProjectName, DocumentType.CodeChurn), Times.Exactly(1));
        }

        [Fact]
        public void WhenGetAllDocumentsByProjectNameAndDocumentTypeShouldReturnExpectedNumberOfDocuments()
        {
            var data = new List<CosmosDataDocument<DailyCodeChurn>> {new CosmosDataDocument<DailyCodeChurn>()};

            dataDocumentRepositoryMock.Setup(x =>
                    x.GetAllDocumentsByProjectAndDocumentType<DailyCodeChurn>(ProjectName, DocumentType.CodeChurn))
                .Returns(data);

            var result = sut.GetAllDocumentsByProjectNameAndDocumentType<DailyCodeChurn>();

            Assert.Single(result);
        }

        private bool CompareWorkitems(WorkItem item1, WorkItem item2)
        {
            return item1.WorkItemId == item2.WorkItemId
                   && item1.ChangesetId == item2.ChangesetId
                   && item1.ClosedDate == item2.ClosedDate;
        }

        private bool CompareDailyCodeChurn(DailyCodeChurn item1, DailyCodeChurn item2)
        {
            return item1.FileName == item2.FileName 
                   && item1.Added == item2.Added 
                   && item1.AddedWithFixes == item2.AddedWithFixes
                   && item1.Authors.Count == item2.Authors.Count 
                   && item1.BugDatabase == item2.BugDatabase
                   && item1.ChangesAfter == item2.ChangesAfter
                   && item1.ChangesAfterWithFixes == item2.ChangesAfterWithFixes
                   && item1.ChangesBefore == item2.ChangesBefore
                   && item1.ChangesBeforeWithFixes == item2.ChangesBeforeWithFixes
                   && item1.Deleted == item2.Deleted
                   && item1.DeletedWithFixes == item2.DeletedWithFixes
                   && item1.Extension == item2.Extension
                   && item1.NumberOfChanges == item2.NumberOfChanges
                   && item1.Timestamp == item2.Timestamp
                   && item1.TotalLinesChanged == item2.TotalLinesChanged
                   && item1.TotalLinesChangedWithFixes == item2.TotalLinesChangedWithFixes;
        }
    }
}
