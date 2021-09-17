using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using vcsparser.core;
using vcsparser.core.Database.Cosmos;
using vcsparser.core.Database.Repository;
using Xunit;

namespace vcsparser.unittests.Database.Cosmos
{
    public class GivenACosmosDbOutputProcessor
    {
        private CosmosDbOutputProcessor sut;

        private readonly string projectName = "some-project-name";

        private Mock<ILogger> loggerMock;
        private Mock<IDataDocumentRepository> dataDocumentRepositoryMock;



        public GivenACosmosDbOutputProcessor()
        {
            this.loggerMock = new Mock<ILogger>();
            dataDocumentRepositoryMock = new Mock<IDataDocumentRepository>();
            sut = new CosmosDbOutputProcessor(loggerMock.Object, dataDocumentRepositoryMock.Object, projectName);
        }


        [Fact]
        public void WhenProcessingSimpleRenameShouldTrackCorrectly()
        {
            var dict = new Dictionary<DateTime, Dictionary<string, DailyCodeChurn>>()
            {
                { new DateTime(2018, 08, 30), new Dictionary<string, DailyCodeChurn>()
                    {
                        {
                            "filename",
                            new DailyCodeChurn()
                            {
                                Added = 1,
                                ChangesBefore = 2,
                                ChangesAfter = 3,
                                Deleted = 4,
                                FileName = "abc",
                                Timestamp = "2018/08/30 00:00:00",
                                Authors = new List<DailyCodeChurnAuthor>() {
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
                            }
                        }
                    }
                }
            };

            var codeChurnData = new CosmosDataDocument<DailyCodeChurn>
            {
                DateTime = new DateTime(2018, 08, 30).ToString(DailyCodeChurn.DATE_FORMAT),
            };

            var listOfItems = new List<CosmosDataDocument<DailyCodeChurn>>()
            {
                codeChurnData
            };

            sut.ProcessOutput(dict);
            //dataDocumentRepositoryMock.Verify(x => x.DeleteMultipleDocuments(It.IsAny<List<CosmosDataDocument<DailyCodeChurn>>>()), Times.Once);
            dataDocumentRepositoryMock.Verify(x => x.DeleteMultipleDocuments(It.Is<List<CosmosDataDocument<DailyCodeChurn>>>(items => 
                items.Any(y => items[0].DateTime == codeChurnData.DateTime))), Times.Once);

        }
    }
}
