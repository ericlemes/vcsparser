using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;
using Xunit;

namespace vcsparser.bugdatabase.azuredevops.unittests
{
    public class GivenAAzureDevOpsRequest
    {
        private Mock<IWebRequest> webRequestMock;

        private DllArgs SomeDllArgs;
        private IAzureDevOpsRequest request;

        private Task<HttpResponseMessage> GetResponseMessage(string response, HttpStatusCode statusCode)
        {
            var message = new HttpResponseMessage(statusCode);
            message.Content = new StringContent(response);
            return Task.Run(() => message);
        }

        public GivenAAzureDevOpsRequest()
        {
            this.webRequestMock = new Mock<IWebRequest>();

            this.SomeDllArgs = new DllArgs
            {
                Organisation = "SomeOrganisation",
                Project = "SomeProject",
                Team = "SomeTeam",
                From = "2019-04-01",
                To = "2019-04-12",
                QueryString = "Some Query from {0} to {1}",
                PersonalAccessToken = "SomePersonalAccessToken"
            };

            this.request = new AzureDevOpsRequest(this.webRequestMock.Object, this.SomeDllArgs);
        }

        [Fact]
        public void WhenSuppliedDllargsPersonalAccessTokenThenConvertToBase64()
        {
            AuthenticationHeaderValue authentication = this.request.Authorization;

            var base64 = authentication.Parameter;
            var bytes = Convert.FromBase64String(base64);
            var token = Encoding.UTF8.GetString(bytes);

            Assert.Equal($":{this.SomeDllArgs.PersonalAccessToken}", token);
        }

        [Fact]
        public void WhenSuppliedDllArgsOrganisationProjectTeamThenConvertToUri()
        {
            Uri workItemListUri = this.request.WorkItemListUri;

            Assert.Equal(
                $"https://dev.azure.com/{this.SomeDllArgs.Organisation}/{this.SomeDllArgs.Project}/{this.SomeDllArgs.Team}/_apis/wit/wiql?api-version=4.1",
                workItemListUri.AbsoluteUri);
        }

        [Fact]
        public void WhenSuppliedDllArgsQueryWithDatesThenConvertToJsonString()
        {
            string json = this.request.JsonQuery;
            dynamic jsonObj = JsonConvert.DeserializeObject(json);
            string jsonQuery = jsonObj.query;

            Assert.Equal(
                string.Format(this.SomeDllArgs.QueryString, this.SomeDllArgs.From, this.SomeDllArgs.To),
                jsonQuery
                );
        }

        [Fact]
        public async Task WhenSendRequestThenSetAuthorizationHeader()
        {
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(
                    JsonConvert.SerializeObject(new { Message = "Some Response Message" }),
                    HttpStatusCode.OK));
            var someHttpRequest = new HttpRequestMessage();

            await this.request.SendRequest<dynamic>(someHttpRequest);

            Assert.Equal(this.request.Authorization, someHttpRequest.Headers.Authorization);
        }

        [Fact]
        public void WhenResponseNotSuccessThenThrowException()
        {
            var statusCode = HttpStatusCode.BadRequest;
            var message = JsonConvert.SerializeObject(new { Message = "Some Response Message" });
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(message, statusCode));
            var someHttpRequest = new HttpRequestMessage();

            Action action = () => this.request.SendRequest<dynamic>(someHttpRequest).Wait();

            var ex = Assert.Throws<AggregateException>(action);
            Assert.Equal($"Response {statusCode}. Content: {message}", ex.InnerException.Message);
        }

        [Fact]
        public async Task WhenResponseSuccessThenDeserializeResponseObject()
        {
            var someResponseMessage = JsonConvert.SerializeObject(new { Message = "Some Response Message" });
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(someResponseMessage, HttpStatusCode.OK));
            var someHttpRequest = new HttpRequestMessage();

            var response = await this.request.SendRequest<object>(someHttpRequest);

            Assert.Equal(someResponseMessage, JsonConvert.SerializeObject(response));
        }


        [Fact]
        public async Task WhenGetFullWorkItemThenNewHttpRequestMessage()
        {
            var someResponseMessage = JsonConvert.SerializeObject(new { Message = "Some Response Message" });
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(someResponseMessage, HttpStatusCode.OK));
            this.webRequestMock.Setup(w => w.NewHttpRequestMessage(It.IsAny<Uri>(), It.IsAny<HttpMethod>()))
                .Returns(new HttpRequestMessage());
            var someUri = new Uri("http://some/uri");

            await this.request.GetFullWorkItem(someUri);

            this.webRequestMock.Verify(w => w.NewHttpRequestMessage(someUri, HttpMethod.Get), Times.Once);
        }

        [Fact]
        public async Task WhenGetFullWorkListThenSetContentHeaders()
        {
            var someResponseMessage = JsonConvert.SerializeObject(new { Message = "Some Response Message" });
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(someResponseMessage, HttpStatusCode.OK));

            var someHttpRequest = new HttpRequestMessage();
            someHttpRequest.RequestUri = new Uri("http://some/uri");
            this.webRequestMock.Setup(w => w.NewHttpRequestMessage(It.IsAny<Uri>(), It.IsAny<HttpMethod>()))
                .Returns(someHttpRequest);

            await this.request.GetWorkItemList();

            Assert.Equal(this.request.JsonQuery, await someHttpRequest.Content.ReadAsStringAsync());
            Assert.Equal(core.bugdatabase.WebRequest.MEDIA_JSON, someHttpRequest.Content.Headers.ContentType.MediaType);
        }

        [Fact]
        public async Task WhenGetFullWorkListThenNewhttpRequestMessage()
        {
            var someResponseMessage = JsonConvert.SerializeObject(new { Message = "Some Response Message" });
            this.webRequestMock.Setup(w => w.Send(It.IsAny<HttpRequestMessage>()))
                .Returns(GetResponseMessage(someResponseMessage, HttpStatusCode.OK));

            var someHttpRequest = new HttpRequestMessage();
            someHttpRequest.RequestUri = new Uri("http://some/uri");
            this.webRequestMock.Setup(w => w.NewHttpRequestMessage(It.IsAny<Uri>(), It.IsAny<HttpMethod>()))
                .Returns(someHttpRequest);

            await this.request.GetWorkItemList();

            this.webRequestMock.Verify(w => w.NewHttpRequestMessage(
                new Uri($"https://dev.azure.com/{this.SomeDllArgs.Organisation}/{this.SomeDllArgs.Project}/{this.SomeDllArgs.Team}/_apis/wit/wiql?api-version=4.1"),
                HttpMethod.Post), Times.Once);
        }
    }
}
