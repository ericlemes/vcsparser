using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class JsonParser<T> : IJsonParser<T> where T : IOutputJson
    {
        private readonly IStreamFactory streamFactory;

        public JsonParser(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public List<T> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);

            var serializer = JsonSerializer.Create();
            var jsonReader = new JsonTextReader(new StreamReader(stream));

            using (stream)
            {
                return serializer.Deserialize<List<T>>(jsonReader);
            }
        }
    }
}
