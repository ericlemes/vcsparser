using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenASonarMeasuresJson
    {
        private SonarMeasuresJson sonarMeasuresJson;

        public GivenASonarMeasuresJson()
        {
            this.sonarMeasuresJson = new SonarMeasuresJson();
        }

        [Fact]
        public void WhenInexistentMeasureShouldReturnNull()
        {
            Assert.Null(this.sonarMeasuresJson.FindMeasure("key", "file1"));
        }

        [Fact]
        public void WhenFindingMeasureForExistingMetricAndInexistentFileShouldReturnNull()
        {
            this.sonarMeasuresJson.AddMeasure(new Measure()
            {
                MetricKey = "key",
                File = "file2"
            });
            Assert.Null(this.sonarMeasuresJson.FindMeasure("key", "file1"));
        }

        [Fact]        
        public void WhenAddingExistingMeasureShouldThrow()
        {
            this.sonarMeasuresJson.AddMeasure(new Measure()
            {
                MetricKey = "key",
                File = "file2"
            });
            Assert.Throws<Exception>(() => {
                this.sonarMeasuresJson.AddMeasure(new Measure()
                {
                    MetricKey = "key",
                    File = "file2"
                });
            });
        }
    }
}
