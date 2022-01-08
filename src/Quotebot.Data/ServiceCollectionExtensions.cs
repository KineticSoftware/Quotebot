using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quotebot.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCosmosDb(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            string endpoint = configuration[ConfigurationConstants.CosmosUrlConfigurationKey];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                Console.WriteLine($"Please specify a valid {ConfigurationConstants.CosmosUrlConfigurationKey} in environment variables");
                return serviceCollection;
            }

            string authKey = configuration[ConfigurationConstants.CosmosAuthorizationConfigurationKey];
            if (string.IsNullOrWhiteSpace(authKey))
            {
                Console.WriteLine($"Please specify a valid {ConfigurationConstants.CosmosAuthorizationConfigurationKey} in the appSettings.json");
                return serviceCollection;
            }

            return serviceCollection
                .AddSingleton(serviceProvider =>
                {
                    CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
                    {
                        Serializer = new CosmosSystemTextJsonSerializer(new JsonSerializerOptions
                        {
                            //PropertyNameCaseInsensitive = true,
                            ReferenceHandler = ReferenceHandler.Preserve
                        })
                    };
                    return new CosmosClient(configuration[ConfigurationConstants.CosmosUrlConfigurationKey], configuration[ConfigurationConstants.CosmosAuthorizationConfigurationKey], cosmosClientOptions);
                })
                .AddSingleton(serviceProvider =>
                {
                    var logger = serviceProvider.GetRequiredService<ILogger<Database>>();
                    var cosmosClient = serviceProvider.GetRequiredService<CosmosClient>();

                    logger.LogInformation("Connecting to database...");
                    logger.LogTrace("CreateDatabaseIfNotExistsAsync");

                    // purposely doing this synchronous. It's required to boostrap the bot. 
                    DatabaseResponse databaseResponse = cosmosClient.CreateDatabaseIfNotExistsAsync(DataConstants.DatabaseId).Result;

                    string output = databaseResponse.StatusCode switch
                    {
                        HttpStatusCode.OK => "Database already existed...",
                        HttpStatusCode.Created => $"Database {DataConstants.DatabaseId} was created successfully",
                        _ => $"An error occured when attempting to create database. Http Status was {databaseResponse.StatusCode}"
                    };

                    logger.LogDebug(output);

                    // The response from Azure Cosmos
                    DatabaseProperties properties = databaseResponse;

                    logger.LogDebug($"Database resource id: {properties.Id} and last modified: {properties.LastModified}");

                    Database database = databaseResponse;

#if DEBUG
                    logger.LogTrace("DeleteContainerStreamAsync");
                    using var deletionResult = database.GetContainer(DataConstants.ContainerId).DeleteContainerStreamAsync().Result;
                    logger.LogTrace($"DeleteContainerStreamAsync {deletionResult.IsSuccessStatusCode}");
#endif

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
                        _ => $"An error occured when attempting to create database. Http Status was {containerResponse.StatusCode}"
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
}
