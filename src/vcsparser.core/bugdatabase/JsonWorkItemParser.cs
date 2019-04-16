using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core.bugdatabase
{
    public interface IJsonWorkItemParser
    {
        List<WorkItem> ParseFile(string fileName);
    }

    public class JsonWorkItemParser : IJsonWorkItemParser
    {
        private readonly IStreamFactory streamFactory;

        public JsonWorkItemParser(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public List<WorkItem> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);

            var serializer = JsonSerializer.Create();
            var jsonReader = new JsonTextReader(new StreamReader(stream));

            using (stream)
            {
                return serializer.Deserialize<List<WorkItem>>(jsonReader);
            }
        }
    }
}
