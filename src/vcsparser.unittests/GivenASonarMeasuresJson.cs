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
        public void WhenInexistentFileMeasureShouldReturnNull()
        {
            Assert.Null(this.sonarMeasuresJson.FindFileMeasure("key", "file1"));
        }

        [Fact]
        public void WhenFindingFileMeasureForExistingMetricAndInexistentFileShouldReturnNull()
        {
            this.sonarMeasuresJson.AddFileMeasure(new Measure<int>()
            {
                MetricKey = "key",
                File = "file2"
            });
            Assert.Null(this.sonarMeasuresJson.FindFileMeasure("key", "file1"));
        }

        [Fact]        
        public void WhenAddingExistingFileMeasureShouldThrow()
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

        [Fact]
        public void WhenInexistentProjectMeasureShouldReturnNull()
        {
            Assert.Null(this.sonarMeasuresJson.FindProjectMeasure("key"));
        }

        [Fact]
        public void WhenFindingProjectMeasureForExistingMetricAndInexistentFileShouldReturnNotNull()
        {
            this.sonarMeasuresJson.AddProjectMeasure(new Measure<int>()
            {
                MetricKey = "key"
            });
            Assert.NotNull(this.sonarMeasuresJson.FindProjectMeasure("key"));
        }

        [Fact]
        public void WhenAddingExistingProjectMeasureShouldThrow()
        {
            this.sonarMeasuresJson.AddProjectMeasure(new Measure<int>()
            {
                MetricKey = "key"
            });
            Assert.Throws<Exception>(() => {
                this.sonarMeasuresJson.AddProjectMeasure(new Measure<int>()
                {
                    MetricKey = "key"
                });
            });
        }
    }
}
