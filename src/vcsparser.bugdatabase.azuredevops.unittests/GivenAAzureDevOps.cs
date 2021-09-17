﻿using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
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
                ClosedDate = someDate,
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
            Assert.Equal(someDate, singleItemValue.ClosedDate);
            Assert.Equal("Some Work Item Id", singleItemValue.WorkItemId);
        }

        [Fact]
        public void WhenProcessWorkItemChangeSetEmptyThenReturnEmpty()
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
                ChangesetId = string.Empty,
                ClosedDate = someDate,
                WorkItemId = "Some Work Item Id"
            });

            var dict = this.azureDevOps.GetWorkItems();

            Assert.Empty(dict);
        }

        [Fact]
        public void WhenProcessWorkItemChangeSetNoneThenReturnEmpty()
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
                ChangesetId = "<None>",
                ClosedDate = someDate,
                WorkItemId = "Some Work Item Id"
            });

            var dict = this.azureDevOps.GetWorkItems();

            Assert.Empty(dict);
        }

        [Fact]
        public void WhenGettingWorkItemsAndWorkItemAlreadyExistsOnDateShouldIgnoreAndNotThrow()
        {
            var fullWorkItem1 = new JObject();
            var fullWorkItem2 = new JObject();

            this.requestMock.Setup(r => r.GetWorkItemList()).Returns(Task.Run(() => new JSONQuery
            {
                WorkItems = new JSONQueryItem[] {
                    new JSONQueryItem {
                        Id = "1",
                        Url = new Uri("http://some/url/1")
                    },
                    new JSONQueryItem {
                        Id = "1",
                        Url = new Uri("http://some/url/1")
                    }
                }
            }));
            this.requestMock.Setup(r => r.GetFullWorkItem(It.Is<Uri>(u => u.AbsoluteUri == "http://some/url/1"))).
                Returns(Task.Run(() => (dynamic)fullWorkItem1));
            this.apiConverterMock.Setup(a => a.ConvertToWorkItem(fullWorkItem1))        
                .Returns(new WorkItem
                {
                    ChangesetId = "Changeset1",
                    ClosedDate = new DateTime(2019, 04, 15),
                    WorkItemId = "1"
                });

            var dict = this.azureDevOps.GetWorkItems();
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
                ClosedDate = someDate,
                WorkItemId = "Some Work Item Id 1"
            }).Returns(new WorkItem
            {
                ChangesetId = "Some Changeset Id 2",
                ClosedDate = someDate,
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
