using Quotebot.Data.Serialization;
using System.Text.Json;

namespace Quotebot.Data.Test;

public class CosmosSystemTextJsonSerializerTests
{
    private readonly CosmosSystemTextJsonSerializer _serializer;

    public CosmosSystemTextJsonSerializerTests()
    {
        var options = new JsonSerializerOptions();
        _serializer = new CosmosSystemTextJsonSerializer(options);
    }

    [Fact]
    public void FromStream_ThrowsException_WhenStreamIsNull()
    {
        // Arrange
        Stream stream = null!;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.FromStream<string>(stream));
    }

    [Fact]
    public void FromStream_ReturnsDefault_WhenStreamIsEmpty()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var result = _serializer.FromStream<string>(stream);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FromStream_ReturnsDeserializedObject_WhenStreamIsNotEmpty()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("\"test\"");
        writer.Flush();
        stream.Position = 0;

        // Act
        var result = _serializer.FromStream<string>(stream);

        // Assert
        Assert.Equal("test", result);
    }

    [Fact]
    public void FromStream_ThrowsException_WhenStreamContainsInvalidData()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("\"test");
        writer.Flush();
        stream.Position = 0;

        // Act & Assert
        Assert.Throws<JsonException>(() => _serializer.FromStream<string>(stream));
    }

    [Fact]
    public void ToStream_ReturnsStream_WhenInputIsNotNull()
    {
        // Arrange
        var input = "test";

        // Act
        var result = _serializer.ToStream(input);

        // Assert
        using var reader = new StreamReader(result);
        Assert.Equal("\"test\"", reader.ReadToEnd());
    }

    [Fact]
    public void FromStream_ThrowsException_WhenDeserializationTypeIsIncorrect()
    {
        // Arrange
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("\"test\"");
        writer.Flush();
        stream.Position = 0;

        // Act & Assert
        Assert.Throws<JsonException>(() => _serializer.FromStream<int>(stream));
    }
}
