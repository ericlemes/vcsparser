using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenAHttpClientWrapperFactory
    {
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<IBugDatabaseFactory> bugDatabaseFactoryMock;

        public GivenAHttpClientWrapperFactory()
        {
            httpClientWrapperMock = new Mock<IHttpClientWrapper>();

            bugDatabaseFactoryMock = new Mock<IBugDatabaseFactory>();
            bugDatabaseFactoryMock.Setup(f => f.GetHttpClientWrapper()).Returns(httpClientWrapperMock.Object);
        }

        [Fact]
        public void WhenGettingSingletonHttpClientWrapper_ThenReturnWrapper()
        {
            var factory = new HttpClientWrapperFactory(bugDatabaseFactoryMock.Object);

            var wrapper = factory.GetSingletonHttpClientWrapper();

            Assert.NotNull(wrapper);
        }

        [Fact]
        public void WhenSameBugFactoryDiffrentWrapper_ThenReturnSingletonWrapper()
        {
            bugDatabaseFactoryMock.SetupSequence(f => f.GetHttpClientWrapper())
                .Returns(new Mock<IHttpClientWrapper>().Object)
                .Throws(new Exception());

            var factory1 = new HttpClientWrapperFactory(bugDatabaseFactoryMock.Object);
            var factory2 = new HttpClientWrapperFactory(bugDatabaseFactoryMock.Object);

            var wrapper1 = factory1.GetSingletonHttpClientWrapper();
            var wrapper2 = factory2.GetSingletonHttpClientWrapper();

            Assert.True(wrapper1.Equals(wrapper2));
        }
    }
}
