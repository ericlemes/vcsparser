using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using vcsparser.core.bugdatabase;
using System.Globalization;

namespace vcsparser.unittests
{
    public class GivenAJsonWorkItemParser
    {
        private JsonParser<WorkItem> jsonWorkItemParser;

        private Mock<IStreamFactory> streamFactory;

        public GivenAJsonWorkItemParser()
        {
            streamFactory = new Mock<IStreamFactory>();
            this.jsonWorkItemParser = new JsonParser<WorkItem>(streamFactory.Object);
        }

        [Fact]
        public void WhenParsingFileShouldReturnExpectedResults()
        {
            var ms = new MemoryStream();
            this.streamFactory.Setup(m => m.readFile("filename")).Returns(ms);

            var sw = new StreamWriter(ms);
            sw.WriteLine("[{\"ClosedDate\":\"2019/04/10 12:00:00\",\"WorkItemId\":\"1\",\"ChangesetId\":\"Some Changeset Id\"}]");
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            var result = this.jsonWorkItemParser.ParseFile("filename");
            var workitem = result.First();
            Assert.Equal("1", workitem.WorkItemId);
            Assert.Equal("Some Changeset Id", workitem.ChangesetId);
            Assert.Equal("2019/04/10 12:00:00", workitem.ClosedDate.ToString(DailyCodeChurn.DATE_FORMAT, CultureInfo.InvariantCulture));
        }
    }
}
