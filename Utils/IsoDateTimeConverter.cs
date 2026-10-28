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


        var parsed = DateTimeOffset.TryParse(rawString!, out DateTimeOffset parsedValue);

        Console.WriteLine("This is the parsed value, {0}", parsedValue);

        if (!parsed) throw new Exception("Could not parse DateTimeOffset");

        return parsedValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        Console.WriteLine("Serializing {0}", value.ToString("o"));

        writer.WriteStringValue(value.ToString("o"));
    }
}
