using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class SonarMeasuresJson
    {
        public SonarMeasuresJson()
        {
            this.Measures = new List<Measure>();
            this.Metrics = new List<Metric>();
        }

        [JsonProperty("measures")]
        public List<Measure> Measures
        {
            get; set;
        }

        [JsonProperty("metrics")]
        public List<Metric> Metrics { get; set; }
    }
}
