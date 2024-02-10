namespace Argon;

public class StringBuilderConverter :
    JsonConverter<StringBuilder>
{
    public override void WriteJson(JsonWriter writer, StringBuilder? value, JsonSerializer serializer) =>
        writer.WriteValue(value);

    public override StringBuilder? ReadJson(JsonReader reader, Type type, StringBuilder? existingValue, bool hasExisting, JsonSerializer serializer)
    {
        if (reader.Value is string value)
        {
            return new(value);
        }

        return null;
    }
}