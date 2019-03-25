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
            Assert.Null(this.sonarMeasuresJson.FindFileMeasure("key", "file1"));
        }

        [Fact]
        public void WhenFindingMeasureForExistingMetricAndInexistentFileShouldReturnNull()
        {
            this.sonarMeasuresJson.AddFileMeasure(new Measure<int>()
            {
                MetricKey = "key",
                File = "file2"
            });
            Assert.Null(this.sonarMeasuresJson.FindFileMeasure("key", "file1"));
        }

        [Fact]        
        public void WhenAddingExistingMeasureShouldThrow()
        {
            this.sonarMeasuresJson.AddFileMeasure(new Measure<int>()
            {
                MetricKey = "key",
                File = "file2"
            });
            Assert.Throws<Exception>(() => {
                this.sonarMeasuresJson.AddFileMeasure(new Measure<int>()
                {
                    MetricKey = "key",
                    File = "file2"
                });
            });
        }
    }
}
