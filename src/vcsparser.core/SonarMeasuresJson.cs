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
            this.MeasuresProject = new List<Measure>();
            this.Metrics = new List<Metric>();
        }

        [JsonProperty("file-measures")]
        public List<Measure> Measures
        {
            get; set;
        }

        [JsonProperty("project-measures")]
        public List<Measure> MeasuresProject {
            get; set;
        }

        private Dictionary<string, Dictionary<string, Measure>> measureFileIndex = new Dictionary<string, Dictionary<string, Measure>>();
        private Dictionary<string, Measure> measureProjectIndex = new Dictionary<string, Measure>();

        [JsonProperty("metrics")]
        public List<Metric> Metrics { get; set; }

        public Measure FindFileMeasure(string metricKey, string fileName)
        {
            if (!measureFileIndex.ContainsKey(metricKey))
                return null;
            if (!measureFileIndex[metricKey].ContainsKey(fileName))
                return null;
            return measureFileIndex[metricKey][fileName];            
        }

        public void AddFileMeasure(Measure measure)
        {
            this.Measures.Add(measure);

            if (!measureFileIndex.ContainsKey(measure.MetricKey))
                measureFileIndex.Add(measure.MetricKey, new Dictionary<string, Measure>());

            if (!measureFileIndex[measure.MetricKey].ContainsKey(measure.File))
                measureFileIndex[measure.MetricKey].Add(measure.File, measure);
            else
                throw new Exception("Measure already exists.");
        }

        public Measure FindProjectMeasure(string metricKey)
        {
            if (!measureProjectIndex.ContainsKey(metricKey))
                return null;
            return measureProjectIndex[metricKey];
        }

        public void AddProjectMeasure(Measure measure)
        {
            this.MeasuresProject.Add(measure);

            if (!measureProjectIndex.ContainsKey(measure.MetricKey))
                measureProjectIndex.Add(measure.MetricKey, measure);
            else
                throw new Exception("Measure already exists.");
        }
    }
}
