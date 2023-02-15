namespace CleanKernel.API.Infrastructure.JsonConverters;

public class ExceptionJsonConverter<TException> : JsonConverter<TException>
        where TException : Exception
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(Exception).IsAssignableFrom(typeToConvert);

    public override TException? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("Deserializing exceptions is not allowed");
    }

    public override void Write(Utf8JsonWriter writer, TException value, JsonSerializerOptions options)
    {
        var ignoreNull = options.DefaultIgnoreCondition == JsonIgnoreCondition.WhenWritingNull;

        writer.WriteStartObject();

        foreach (var prop in typeof(TException).GetProperties())
        {
            if (prop.Name == nameof(Exception.TargetSite))
            {
                continue;
            }

            var propValue = prop.GetValue(value);

            if (ignoreNull && propValue is null)
            {
                continue;
            }

            writer.WritePropertyName(options.PropertyNamingPolicy?.ConvertName(prop.Name) ?? prop.Name);
            JsonSerializer.Serialize(writer, propValue, options);
        }

        writer.WriteEndObject();
    }
}
