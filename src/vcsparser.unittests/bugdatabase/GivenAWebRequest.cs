using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Moq;
using vcsparser.core.bugdatabase;
using System.Net.Http;

namespace vcsparser.unittests.bugdatabase
{
    public class GivenAWebRequest
    {
        private Mock<IHttpClientWrapper> httpClientWrapperMock;
        private Mock<IHttpClientWrapperFactory> httpClientWrapperFactoryMock;
        private WebRequest webRequest;

        public GivenAWebRequest()
        {
            var response = new HttpResponseMessage();
            var responeTask = Task.Run(() => response);

            this.httpClientWrapperMock = new Mock<IHttpClientWrapper>();
            this.httpClientWrapperMock.Setup((h) => h.SendAsync(It.IsAny<HttpRequestMessage>())).Returns(responeTask);

            this.httpClientWrapperFactoryMock = new Mock<IHttpClientWrapperFactory>();
            this.httpClientWrapperFactoryMock.Setup((f) => f.GetSingletonHttpClientWrapper()).Returns(this.httpClientWrapperMock.Object);

            this.webRequest = new WebRequest(this.httpClientWrapperFactoryMock.Object);
        }

        [Fact]
        public void WhenNewHttpRequestMessageThenSetupCorrectly()
        {
            Uri someUri = new Uri("http://some/uri");
            HttpMethod someMethod = HttpMethod.Get;

            var request = this.webRequest.NewHttpRequestMessage(someUri, someMethod);

            Assert.Equal(someUri, request.RequestUri);
            Assert.Equal(someMethod, request.Method);
        }

        [Fact]
        public void WhenNewHttpRequestMessageThenSetupCorrectlyWithHeaders()
        {
            Uri someUri = new Uri("http://some/uri");
            HttpMethod someMethod = HttpMethod.Get;
            string someMediaType = "some/mediaType";

            var request = this.webRequest.NewHttpRequestMessage(someUri, someMethod, someMediaType);

            Assert.Equal(someMediaType, request.Headers.Accept.Single().MediaType);
        }

        [Fact]
        public async Task WhenSendThenSendMessage()
        {
            Uri someUri = new Uri("http://some/uri");
            HttpMethod someMethod = HttpMethod.Get;

            var request = this.webRequest.NewHttpRequestMessage(someUri, someMethod);

            await this.webRequest.Send(request);

            this.httpClientWrapperMock.Verify((h) => h.SendAsync(request), Times.Once);
        }
    }
}
