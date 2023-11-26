global using Xunit;
using Microsoft.Extensions.Logging;
using SourceMock;
using Discord;


[assembly: GenerateMocksForAssemblyOf(typeof(ILogger))]
[assembly: GenerateMocksForAssemblyOf(typeof(IMessage))]
