using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.bugdatabase.azuredevops.unittests
{
    public class GivenABugDatabaseProvider
    {
        private Mock<ILogger> loggerMock;
        private Mock<IWebRequest> webRequestMock;
        private Mock<IAzureDevOps> azureDevOpsMock;
        private Mock<IAzureDevOpsFactory> azureDevOpsFactoryMock;

        private IEnumerable<string> dllArgs;
        private BugDatabaseProvider provider;

        public GivenABugDatabaseProvider()
        {
            this.loggerMock = new Mock<ILogger>();
            this.webRequestMock = new Mock<IWebRequest>();

            this.azureDevOpsMock = new Mock<IAzureDevOps>();
            this.azureDevOpsMock.Setup(a => a.GetWorkItems()).Returns(new WorkItemList());

            this.azureDevOpsFactoryMock = new Mock<IAzureDevOpsFactory>();
            this.azureDevOpsFactoryMock.Setup(f => f.GetAzureDevOps(It.IsAny<ILogger>(), It.IsAny<IAzureDevOpsRequest>(), It.IsAny<IApiConverter>(), It.IsAny<ITimeKeeper>())).Returns(this.azureDevOpsMock.Object);

            this.dllArgs = new string[] {
                "--organisation", "SomeOrganisation",
                "--project", "SomeProject",
                "--team", "SomeTeam",
                "--from", "2019-04-01",
                "--to", "2019-04-12",
                "--token", "SomePersonalAccessToken"
            };

            this.provider = new BugDatabaseProvider();
            this.provider.SetLogger(this.loggerMock.Object);
            this.provider.SetWebRequest(this.webRequestMock.Object);
            this.provider.SetAzureDevOpsFactory(this.azureDevOpsFactoryMock.Object);
        }

        [Fact]
        public void WhenProcessArgsFail_ThenReturnOne()
        {
            var code = this.provider.ProcessArgs(new string[] { "--some", "args", "--that", "are", "--wrong" });

            Assert.Equal(1, code);
        }

        [Fact]
        public void WhenProcessArgsDatesInWrongFormat_ThenReturnOne()
        {
            this.dllArgs = new string[] {
                "--organisation", "SomeOrganisation",
                "--project", "SomeProject",
                "--team", "SomeTeam",
                "--from", "01-04-2019",
                "--to", "12-04-2019",
                "--token", "SomePersonalAccessToken"
            };
            var code = this.provider.ProcessArgs(this.dllArgs);

            Assert.Equal(1, code);
            this.loggerMock.Verify(l => l.LogToConsole(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void WhenProcessArgsParsed_ThenReturnZero()
        {
            var code = this.provider.ProcessArgs(this.dllArgs);

            Assert.Equal(0, code);
        }

        [Fact]
        public void WhenProcessBeforeProcessArgs_ThenReturnNull()
        {
            var workItemList = this.provider.Process();

            Assert.Null(workItemList);
        }

        [Fact]
        public void WhenProcess_ThenWorkItemList()
        {
            this.provider.ProcessArgs(this.dllArgs);
            var workItemList = this.provider.Process();

            this.azureDevOpsMock.Verify(a => a.GetWorkItems(), Times.Once);
        }
    }
}
