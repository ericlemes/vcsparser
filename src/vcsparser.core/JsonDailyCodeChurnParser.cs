using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class JsonDailyCodeChurnParser : IDailyCodeChurnParser
    {
        private IStreamFactory streamFactory;

        public JsonDailyCodeChurnParser(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public IList<DailyCodeChurn> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);

            var serializer = JsonSerializer.Create();
            var jsonReader = new JsonTextReader(new StreamReader(stream));

            using (stream)
            {
                var content = serializer.Deserialize<JsonOutputData>(jsonReader);
                if (content.SchemaVersion == OutputProcessor.SchemaVersion)
                    return content.Data;
               throw new Exception($"Version mismatch. Expecting {OutputProcessor.SchemaVersion} found {content.SchemaVersion}");
            }
        }
    }
}
