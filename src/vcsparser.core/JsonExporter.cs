using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class JsonExporter : IJsonExporter
    {
        private IStreamFactory streamFactory;

        public JsonExporter(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public void Export<T>(T dataToExport, string outputFile)
        {
            var stream = streamFactory.createFileStream(outputFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            var streamWriter = new StreamWriter(stream);

            using (streamWriter)
            {
                var serializer = Newtonsoft.Json.JsonSerializer.Create();
                serializer.Serialize(streamWriter, dataToExport);
                streamWriter.Flush();
            }
        }
    }
}
