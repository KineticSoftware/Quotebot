using System.Reflection;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Mocks;
using Quotebot.Data.Serialization;


namespace Quotebot.Data.Test;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    //[Fact(Skip = "You must have Azure Cosmos Emulator Running")]
    public void RegisterCosmosDb_Should_Register_Required_Services()
    {
        LoggerMock<Database> mockDatabaseLogger = new();
        LoggerMock<CosmosDbService> mockCosmosDbServiceLogger = new();
        LoggerMock<Container> mockContainerLogger = new();


        IConfiguration configuration = new ConfigurationBuilder()
            // ReSharper disable once StringLiteralTypo
            .AddJsonFile("appsettings.json")
            .AddUserSecrets(Assembly.GetExecutingAssembly(), false)
            .Build();

        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<ILogger<Database>>(mockDatabaseLogger);
        serviceCollection.AddSingleton<ILogger<CosmosDbService>>(mockCosmosDbServiceLogger);
        serviceCollection.AddSingleton<ILogger<Container>>(mockContainerLogger);
        serviceCollection.RegisterCosmosDb(configuration);
        ServiceProvider actualServiceProvider = serviceCollection.BuildServiceProvider();
        Assert.NotNull(actualServiceProvider.GetRequiredService<CosmosClient>());
        Assert.IsType<CosmosSystemTextJsonSerializer>(actualServiceProvider.GetRequiredService<CosmosClient>().ClientOptions.Serializer);
        Assert.NotNull(actualServiceProvider.GetRequiredService<Database>());
        Assert.NotNull(actualServiceProvider.GetRequiredService<Container>());
        Assert.NotNull(actualServiceProvider.GetRequiredService<IDataService>());
    }

    [Fact]
    public void RegisterCosmosDb_Should_Throw_If_ConfigurationSectionName_Not_Found()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        ServiceCollection serviceCollection = new();
        Assert.Throws<InvalidOperationException>(() => serviceCollection.RegisterCosmosDb(configuration));
    }

    [Fact]
    public void RegisterCosmosDb_Should_Throw_If_CosmosDb_Configuration_Url_Is_WhiteSpace()
    {
        IReadOnlyDictionary<string, string> appSettingsStub = new Dictionary<string, string>()
        {
            { "CosmosDb:Url", ""}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettingsStub!)
            .Build();

        ServiceCollection serviceCollection = new();
        Assert.Throws<ArgumentException>(() => serviceCollection.RegisterCosmosDb(configuration));
    }

    [Fact]
    public void RegisterCosmosDb_Should_Throw_If_CosmosDb_Configuration_Authorization_Is_WhiteSpace()
    {
        IReadOnlyDictionary<string, string> appSettingsStub = new Dictionary<string, string>()
        {
            {"CosmosDb:Url", "http://test.it"},
            {"CosmosDb:Authorization", ""}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(appSettingsStub!)
            .Build();

        ServiceCollection serviceCollection = new();
        Assert.Throws<ArgumentException>(() => serviceCollection.RegisterCosmosDb(configuration));
    }
}