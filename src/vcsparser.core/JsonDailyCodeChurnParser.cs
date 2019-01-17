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

        public List<DailyCodeChurn> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);
            
            var serializer = JsonSerializer.Create();
            var jsonReader = new JsonTextReader(new StreamReader(stream));            

            using (stream)
            {
                return serializer.Deserialize<List<DailyCodeChurn>>(jsonReader);
            }
        }
    }
}
