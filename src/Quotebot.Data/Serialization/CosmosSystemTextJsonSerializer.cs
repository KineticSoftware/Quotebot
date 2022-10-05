//using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;

namespace Quotebot.Data.Serialization;

public class CosmosSystemTextJsonSerializer : CosmosSerializer
{
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    //private readonly JsonObjectSerializer _systemTextJsonSerializer;

    public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        //_systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);
        _jsonSerializerOptions = jsonSerializerOptions;
    }

    public override T FromStream<T>(Stream stream)
    {
        if (stream is null)
            throw new ArgumentException("Stream is null", nameof(stream));

        if (stream.CanSeek && stream.Length == 0)
        {
            return default!;
        }

        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
            return (T)(object)stream;
        }
        using (stream)
        {
            var buffer = JsonSerializer.Deserialize<T>(stream, _jsonSerializerOptions);

            if (buffer is null)
                throw new NullReferenceException(nameof(buffer));

            if (buffer is { } result)
                return result;

            throw new Exception($"Unable to convert {buffer.GetType().FullName} to {typeof(T).FullName}");
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var streamPayload = new MemoryStream();
        JsonSerializer.Serialize(streamPayload, input, _jsonSerializerOptions);
        streamPayload.Position = 0;
        return streamPayload;
    }
}
