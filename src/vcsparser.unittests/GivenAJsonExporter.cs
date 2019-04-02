using Moq;
using vcsparser.core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAJsonExporter
    {
        [Fact]
        public void WhenExportingShouldGenerateContentToFile()
        {
            var mockStreamFactory = new Mock<IStreamFactory>();
            var memoryStream = new MemoryStream();
            mockStreamFactory.Setup(m => m.createFileStream("filename", FileMode.Create, FileAccess.Write)).Returns(memoryStream);

            var measures = new SonarMeasuresJson()
            {
                Measures = new List<IMeasure>()
                {
                    new Measure<int>()
                    {
                        MetricKey = "key",
                        File = "file",
                        Value = 1
                    }
                }
            };

            var jsonExporter = new JsonExporter(mockStreamFactory.Object);
            jsonExporter.Export(measures, "filename");
            
            var resultString = UTF8Encoding.UTF8.GetString(memoryStream.GetBuffer());

            Assert.NotEmpty(resultString);
        }
    }
}
