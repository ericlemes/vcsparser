using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class CsvParser : ICsvParser
    {
        private IStreamFactory streamFactory;

        public CsvParser(IStreamFactory streamFactory)
        {
            this.streamFactory = streamFactory;
        }

        public List<DailyCodeChurn> ParseFile(string fileName)
        {
            var stream = streamFactory.readFile(fileName);
            using (stream)
            {
                var streamReader = new StreamReader(stream);
                var configuration = new CsvHelper.Configuration.Configuration();
                configuration.HeaderValidated = null;
                configuration.MissingFieldFound = null;                
                var csvReader = new CsvReader(streamReader, configuration);
                
                return csvReader.GetRecords<DailyCodeChurn>().ToList();
            }
        }
    }
}
