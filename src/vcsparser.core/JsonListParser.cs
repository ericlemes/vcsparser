using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core.bugdatabase;

namespace vcsparser.core
{
    public class JsonListParser<T> : IJsonListParser<T> where T : IOutputJson
    {
        private readonly IStreamFactory streamFactory;

        public JsonListParser(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public List<T> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);

            var serializer = JsonSerializer.Create();
            serializer.Converters.Add(new JsonDateTimeCustomConverter(DailyCodeChurn.DATE_FORMAT, CultureInfo.InvariantCulture));
            var jsonReader = new JsonTextReader(new StreamReader(stream));

            using (stream)
            {
                return serializer.Deserialize<List<T>>(jsonReader);
            }
        }
    }
}
