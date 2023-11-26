using Microsoft.Azure.Cosmos;

namespace Quotebot.Data;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection RegisterCosmosDb(this IServiceCollection serviceCollection, IConfiguration parentConfiguration)
    {
        CosmosConfiguration configuration = parentConfiguration.GetRequiredSection(CosmosConfiguration.ConfigurationSectionName).Get<CosmosConfiguration>()!;

        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.Url);
        ArgumentException.ThrowIfNullOrWhiteSpace(configuration.Authorization);

        return serviceCollection
            .AddSingleton<CosmosClient>(_ =>
            {
                CosmosClientOptions cosmosClientOptions = new()
                {
                    Serializer = new CosmosSystemTextJsonSerializer(new())
                };
                return new(configuration.Url, configuration.Authorization, cosmosClientOptions);
            })
            .AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Database>>();
                var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();

                logger.LogInformation("Connecting to database...");
                logger.LogTrace("CreateDatabaseIfNotExistsAsync");

                // purposely doing this synchronous. It's required to bootstrap the bot. 
                DatabaseResponse databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(DataConstants.DatabaseId).Result;

                string output = databaseResponse.StatusCode switch
                {
                    HttpStatusCode.OK => "Database already existed...",
                    HttpStatusCode.Created => $"Database {DataConstants.DatabaseId} was created successfully",
                    _ => $"An error occurred when attempting to create database. Http Status was {databaseResponse.StatusCode}"
                };

                logger.LogDebug(output);

                // The response from Azure Cosmos
                DatabaseProperties properties = databaseResponse;

                logger.LogDebug($"Database resource id: {properties.Id} and last modified: {properties.LastModified}");

                Database database = databaseResponse;

                if (configuration.AlwaysRebuildContainer)
                {
                    logger.LogCritical("DeleteContainerStreamAsync");
                    using var deletionResult = database.GetContainer(DataConstants.ContainerId).DeleteContainerStreamAsync().Result;
                    logger.LogCritical($"DeleteContainerStreamAsync {deletionResult.IsSuccessStatusCode}");
                }

                return database;
            })
            .AddSingleton(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<Container>>();
                var database = serviceProvider.GetRequiredService<Database>();

                logger.LogTrace("CreateContainerIfNotExistsAsync");

                ContainerResponse containerResponse = database.CreateContainerIfNotExistsAsync(
                id: DataConstants.ContainerId,
                partitionKeyPath: DataConstants.PrimaryPartitionKey, 1000).Result;

                string output = containerResponse.StatusCode switch
                {
                    HttpStatusCode.OK => "Container already existed...",
                    HttpStatusCode.Created => $"Container {DataConstants.ContainerId} was created successfully",
                    _ => $"An error occurred when attempting to create database. Http Status was {containerResponse.StatusCode}"
                };

                logger.LogDebug(output);

                ContainerProperties properties = containerResponse;
                logger.LogDebug($"Container resource id: {properties.Id} and last modified: {properties.LastModified}");

                Container container = containerResponse;

                return container;
            })
            .AddSingleton<IDataService, CosmosDbService>();
    }
}
