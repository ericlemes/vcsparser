using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAJsonSerializerSettingsFactory
    {
        [Fact]
        public void WhenCreateDefaultSerializerSettingsForCosmosDBShouldSerializerHaveExpectedValuesSet()
        {
            var settings = JsonSerializerSettingsFactory.CreateDefaultSerializerSettingsForCosmosDB();
            Assert.Equal(DateFormatHandling.IsoDateFormat, settings.DateFormatHandling);
            Assert.Equal(DateTimeZoneHandling.RoundtripKind, settings.DateTimeZoneHandling);
            Assert.Equal(DateFormatHandling.IsoDateFormat, settings.DateFormatHandling);
            Assert.All(settings.Converters, x => Assert.IsAssignableFrom<StringEnumConverter>(x));
        }
    }
}
