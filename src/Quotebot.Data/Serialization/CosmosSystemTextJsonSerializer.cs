//using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;

namespace Quotebot.Data.Serialization;

public class CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions) : CosmosSerializer
{
    public override T FromStream<T>(Stream stream)
    {
        if (stream is null)
            throw new ArgumentException("Stream is null", nameof(stream));

        if (stream is {CanSeek: true, Length: 0})
        {
            return default!;
        }

        if (typeof(Stream).IsAssignableFrom(typeof(T)))
        {
            return (T)(object)stream;
        }
        using (stream)
        {
            var buffer = JsonSerializer.Deserialize<T>(stream, jsonSerializerOptions);

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
        JsonSerializer.Serialize(streamPayload, input, jsonSerializerOptions);
        streamPayload.Position = 0;
        return streamPayload;
    }
}
