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

        private Dictionary<string, Dictionary<string, Measure>> measureIndex = new Dictionary<string, Dictionary<string, Measure>>();

        [JsonProperty("metrics")]
        public List<Metric> Metrics { get; set; }

        public Measure FindMeasure(string metricKey, string fileName)
        {
            if (!measureIndex.ContainsKey(metricKey))
                return null;
            if (!measureIndex[metricKey].ContainsKey(fileName))
                return null;
            return measureIndex[metricKey][fileName];            
        }

        public void AddMeasure(Measure measure)
        {
            this.Measures.Add(measure);
            if (!measureIndex.ContainsKey(measure.MetricKey))
                measureIndex.Add(measure.MetricKey, new Dictionary<string, Measure>());
            if (!measureIndex[measure.MetricKey].ContainsKey(measure.File))
                measureIndex[measure.MetricKey].Add(measure.File, measure);
            else
                throw new Exception("Measure already exists.");
        }
    }
}
