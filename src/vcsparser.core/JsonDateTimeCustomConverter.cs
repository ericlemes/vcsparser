using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vcsparser.core
{
    public class JsonDateTimeCustomConverter : DateTimeConverterBase
    {
        private readonly string format;
        private readonly CultureInfo culture;

        public JsonDateTimeCustomConverter(string format, CultureInfo culture)
        {
            this.format = format;
            this.culture = culture;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTime);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTime.ParseExact(reader.Value.ToString(), format, culture, DateTimeStyles.None);
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(Convert.ToDateTime(value).ToString(format, culture));
            writer.Flush();
        }
    }
}
