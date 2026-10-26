using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigitalWallet.Utils;

public class IsoDateTimeConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetDateTimeOffset(out DateTimeOffset value))
        {
            return value;
        }

        string? rawString = reader.GetString();
        Console.WriteLine("Deserializing {0}", rawString);
        return DateTimeOffset.Parse(rawString!);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        Console.WriteLine("Serializing {0}", value.ToString("o"));

        writer.WriteStringValue(value.ToString("o", CultureInfo.InvariantCulture));
    }
}
