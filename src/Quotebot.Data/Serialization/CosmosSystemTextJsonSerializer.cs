using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

namespace Quotebot.Data.Serialization;

public class CosmosSystemTextJsonSerializer : CosmosSerializer
{
    private readonly JsonObjectSerializer _systemTextJsonSerializer;

    public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        _systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);
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
            var buffer = _systemTextJsonSerializer.Deserialize(stream, typeof(T), default);

            if (buffer is null)
                throw new NullReferenceException(nameof(buffer));

            if (buffer is T result)
                return result;

            throw new Exception($"Unable to convert {buffer.GetType().FullName} to {typeof(T).FullName}");
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var streamPayload = new MemoryStream();
        _systemTextJsonSerializer.Serialize(streamPayload, input, typeof(T), default);
        streamPayload.Position = 0;
        return streamPayload;
    }
}
