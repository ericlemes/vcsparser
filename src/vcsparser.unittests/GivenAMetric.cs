using vcsparser.core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAMetric
    {
        [Fact]
        public void WhenCreatingShouldInitialiseProperly()
        {
            var metric = new Metric()
            {
                MetricKey = "key",
                Name = "name",
                Description = "description",
                Type = "INT"
            };
            Assert.Equal("key", metric.MetricKey);
            Assert.Equal("name", metric.Name);
            Assert.Equal("description", metric.Description);
            Assert.Equal("INT", metric.Type);
            Assert.Equal(0, metric.Direction);
            Assert.False(metric.Qualitative);
            Assert.Equal("Code churn", metric.Domain);

        }
    }
}
