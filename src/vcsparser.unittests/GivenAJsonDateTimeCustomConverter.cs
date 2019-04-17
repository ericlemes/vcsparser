using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vcsparser.core;
using Xunit;

namespace vcsparser.unittests
{
    public class GivenAJsonDateTimeCustomConverter
    {
        private string someFormat;
        private CultureInfo someCulture;
        private JsonSerializer someJsonSerializer;
        private JsonDateTimeCustomConverter converter;

        private Mock<JsonReader> jsonReaderMock;
        private Mock<JsonWriter> jsonWriterMock;

        public GivenAJsonDateTimeCustomConverter()
        {
            someFormat = "yyyy/MM/dd HH:mm:ss";
            someCulture = CultureInfo.InvariantCulture;
            someJsonSerializer = new JsonSerializer();
            converter = new JsonDateTimeCustomConverter(someFormat, someCulture);

            jsonReaderMock = new Mock<JsonReader>();
            jsonWriterMock = new Mock<JsonWriter>();
        }

        [Fact]
        public void GivenCanConvertDateTimeTypeThenReturnTrue()
        {
            var type = typeof(DateTime);
            var canConvert = converter.CanConvert(type);

            Assert.True(canConvert);
        }

        [Fact]
        public void GivenCanConvertOtherTypeThenReturnFalse()
        {
            var someOtherType = this.GetType();

            var canConvert = converter.CanConvert(someOtherType);

            Assert.False(canConvert);
        }

        [Fact]
        public void GivenReadJsonDateCorrectFormatThenParserExact()
        {
            var someObjectType = typeof(DateTime);
            var someExisitingValue = new DateTime();
            jsonReaderMock.Setup(j => j.Value).Returns("2019/04/17 12:00:00");

            var dateTimeObj = converter.ReadJson(jsonReaderMock.Object, someObjectType, someExisitingValue, someJsonSerializer);

            Assert.IsType<DateTime>(dateTimeObj);
            var dateTime = (DateTime)dateTimeObj;
            Assert.Equal("2019/04/17 12:00:00", dateTime.ToString(someFormat, someCulture));
        }

        [Fact]
        public void GivenReadJsonDateIncorrectFormatThenParserExact()
        {
            var someObjectType = typeof(DateTime);
            var someExisitingValue = new DateTime();
            jsonReaderMock.Setup(j => j.Value).Returns("12:00:00 2019/04/17");

            Action ReadJson = () => converter.ReadJson(jsonReaderMock.Object, someObjectType, someExisitingValue, someJsonSerializer);

            var exception = Assert.Throws<FormatException>(ReadJson);
            Assert.Equal("String was not recognized as a valid DateTime.", exception.Message);
        }

        [Fact]
        public void GivenWriteJsonValueDateTimeThenConvertToFormat()
        {
            var someDateTime = new DateTime(2019, 04, 17, 12, 00, 00);

            converter.WriteJson(jsonWriterMock.Object, someDateTime, someJsonSerializer);

            jsonWriterMock.Verify(j => j.WriteValue("2019/04/17 12:00:00"), Times.Once);
        }

        [Fact]
        public void GivenWriteJsonValueStringThenConvertToFormat()
        {
            var someValue = "Some String that is not in DateTimeFormat";

            Action writeJson = () => converter.WriteJson(jsonWriterMock.Object, someValue, someJsonSerializer);

            var exception = Assert.Throws<FormatException>(writeJson);
            Assert.StartsWith("The string was not recognized as a valid DateTime.", exception.Message);
        }

        [Fact]
        public void GivenWriteJsonValueNotDateTimeThenConvertToFormat()
        {
            var someValue = 132456;

            Action writeJson = () => converter.WriteJson(jsonWriterMock.Object, someValue, someJsonSerializer);

            Assert.Throws<InvalidCastException>(writeJson);
        }
    }
}
