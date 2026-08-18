using System.Text.Json;
using System.Text.Json.Serialization;

namespace Apha.FPS.Api.Converters
{
    /// <summary>
    /// Serialises <see cref="decimal"/> values with a minimum of 2 decimal places
    /// and a maximum of 4, stripping only trailing zeros beyond the 2nd place.
    /// e.g. 12.9600 → 12.96  |  12.9672 → 12.9672  |  12.0000 → 12.00
    /// Deserialisation is unchanged — full precision round-trips correctly.
    /// </summary>
    public class TrimDecimalJsonConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.GetDecimal();

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
            => writer.WriteNumberValue(decimal.Parse(value.ToString("0.00##")));
    }
}