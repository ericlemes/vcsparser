using Moq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.bugdatabase.azuredevops.unittests
{
    public class GivenAAzureDevOps
    {
        private IAzureDevOpsFactory devOpsFactory;
        private IAzureDevOps azureDevOps;

        private Mock<ILogger> loggerMock;
        private Mock<IAzureDevOpsRequest> requestMock;
        private Mock<IApiConverter> apiConverterMock;
        private Mock<ITimeKeeper> timeKeeperMock;

        public GivenAAzureDevOps()
        {
            this.loggerMock = new Mock<ILogger>();
            this.requestMock = new Mock<IAzureDevOpsRequest>();
            this.apiConverterMock = new Mock<IApiConverter>();
            this.timeKeeperMock = new Mock<ITimeKeeper>();

            this.devOpsFactory = new AzureDevOpsFactory();
            this.azureDevOps = new AzureDevOps(this.loggerMock.Object, this.requestMock.Object, this.apiConverterMock.Object, this.timeKeeperMock.Object);
        }

        [Fact]
        public void WhenProcessWorkItemsEmptyQueryThenReturnEmpty()
        {
            this.requestMock.Setup(r => r.GetWorkItemList()).Returns(Task.Run(() => new JSONQuery
            {
                WorkItems = new JSONQueryItem[0]
            }));

            var dict = this.azureDevOps.GetWorkItems();

            Assert.Empty(dict.Keys);
        }

        [Fact]
        public void WhenProcessWorkItemWithItemThenReturnWorkItem()
        {
            this.requestMock.Setup(r => r.GetWorkItemList()).Returns(Task.Run(() => new JSONQuery
            {
                WorkItems = new JSONQueryItem[] { new JSONQueryItem {
                    Id = "Some Id",
                    Url = new Uri("http://some/url")
                }}
            }));
            this.requestMock.Setup(r => r.GetFullWorkItem(It.IsAny<Uri>())).Returns(Task.Run(() => (dynamic)null));
            var someDate = new DateTime(2019, 04, 15);
            this.apiConverterMock.Setup(a => a.ConvertToWorkItem(It.IsAny<object>())).Returns(new WorkItem
            {
                ChangesetId = "Some Changeset Id",
                ClosedDate = someDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                WorkItemId = "Some Work Item Id"
            });

            var dict = this.azureDevOps.GetWorkItems();

            var singleKey = Assert.Single(dict.Keys);
            Assert.Equal(someDate, singleKey);

            var singleValue = Assert.Single(dict.Values);

            var singleItemKey = Assert.Single(singleValue.Keys);
            Assert.Equal("Some Changeset Id", singleItemKey);

            var singleItemValue = Assert.Single(singleValue.Values);

            Assert.Equal("Some Changeset Id", singleItemValue.ChangesetId);
            Assert.Equal(someDate, singleItemValue.GetClosedDateAsDateTime());
            Assert.Equal("Some Work Item Id", singleItemValue.WorkItemId);
        }

        [Fact]
        public void WhenProcessWorkItemWithItemAndExcptionThenReturnSingleWorkItem()
        {
            this.requestMock.Setup(r => r.GetWorkItemList()).Returns(Task.Run(() => new JSONQuery
            {
                WorkItems = new JSONQueryItem[] {
                    new JSONQueryItem {
                        Id = "Some Id 1",
                        Url = new Uri("http://some/url/1")
                    },
                    new JSONQueryItem {
                        Id = "Some Id 2",
                        Url = new Uri("http://some/url/2")
                    }
                }
            }));
            this.requestMock.Setup(r => r.GetFullWorkItem(It.IsAny<Uri>())).Returns(Task.Run(() => (dynamic)null));
            this.apiConverterMock.SetupSequence(a => a.ConvertToWorkItem(It.IsAny<object>())).Returns(new WorkItem
            {
                ChangesetId = "Some Changeset Id",
                ClosedDate = new DateTime(2019, 04, 15).ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                WorkItemId = "Some Work Item Id"
            }).Throws(new Exception("Some Exception!"));

            var dict = this.azureDevOps.GetWorkItems();

            var byDate = Assert.Single(dict.Values);
            Assert.Single(byDate.Values);
            this.loggerMock.Verify(l => l.LogToConsole("Error Processing Work Item 'Some Id 2': Some Exception!"), Times.Once);
        }

        [Fact]
        public void WhenProcessWorkItemsWithSameDateThenReturnWorkItem()
        {
            this.requestMock.Setup(r => r.GetWorkItemList()).Returns(Task.Run(() => new JSONQuery
            {
                WorkItems = new JSONQueryItem[] {
                    new JSONQueryItem {
                        Id = "Some Id 1",
                        Url = new Uri("http://some/url/1")
                    },
                    new JSONQueryItem {
                        Id = "Some Id 2",
                        Url = new Uri("http://some/url/2")
                    }
                }
            }));
            this.requestMock.Setup(r => r.GetFullWorkItem(It.IsAny<Uri>())).Returns(Task.Run(() => (dynamic)null));
            var someDate = new DateTime(2019, 04, 15);
            this.apiConverterMock.SetupSequence(a => a.ConvertToWorkItem(It.IsAny<object>())).Returns(new WorkItem
            {
                ChangesetId = "Some Changeset Id 1",
                ClosedDate = someDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                WorkItemId = "Some Work Item Id 1"
            }).Returns(new WorkItem
            {
                ChangesetId = "Some Changeset Id 2",
                ClosedDate = someDate.ToString("yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture),
                WorkItemId = "Some Work Item Id 2"
            });

            var dict = this.azureDevOps.GetWorkItems();

            var singleDateKey = Assert.Single(dict.Keys);
            Assert.Equal(someDate, singleDateKey);

            var singleDateValue = Assert.Single(dict.Values);

            Assert.Equal(2, singleDateValue.Count);
        }
    }
}
