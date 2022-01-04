using Discord;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Quotebot.Data
{
    public class CosmosDbService : IDataService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly ILogger _logger;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        //// The container we will create.
        private Container? _container;

        //// The name of the database and container we will create
        private readonly string _databaseId = "Quotebot";
        private readonly string _containerId = "Quotes";
        private readonly string _partitionKeyPath = "/Author/Id";

        public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;

            _jsonSerializerOptions = new(JsonSerializerDefaults.General)
            {
                UnknownTypeHandling = JsonUnknownTypeHandling.JsonElement
            };
        }

        public async Task Initialize()
        {
            try
            {
                _logger.LogInformation("Connecting to database...");
                Database? database = await InitializeDatabase();
                if (database == null)
                {
                    throw new ArgumentNullException(nameof(database));
                }
                await DeleteExistingContainerIfDebug(database);
                _container = await InitializeContainer(database);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(Convert.ToString(ex));
            }
        }

        private async Task<Database> InitializeDatabase()
        {
            _logger.LogTrace("CreateDatabaseIfNotExistsAsync");
            DatabaseResponse databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);

            string output = databaseResponse.StatusCode switch
            {
                HttpStatusCode.OK => "Database already existed...",
                HttpStatusCode.Created => $"Database {_databaseId} was created successfully",
                _ => $"An error occured when attempting to create database. Http Status was {databaseResponse.StatusCode}"
            };

            _logger.LogDebug(output);

            // The response from Azure Cosmos
            DatabaseProperties properties = databaseResponse;

            _logger.LogDebug($"Database resource id: {properties.Id} and last modified: {properties.LastModified}");

            return databaseResponse;
        }
        
        private async Task DeleteExistingContainerIfDebug(Database database)
        {
#if DEBUG
            // Delete the existing container to prevent create item conflicts
            using (await database.GetContainer(_containerId).DeleteContainerStreamAsync())
            { }
#else
            await Task.CompletedTask;
#endif
        }

        private async Task<Container> InitializeContainer(Database database)
        {
            _logger.LogTrace("CreateContainerIfNotExistsAsync");
            // Set throughput to the minimum value of 400 RU/s
            ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(
                id: _containerId,
                partitionKeyPath: _partitionKeyPath,
                throughput: 400);

            string output = containerResponse.StatusCode switch
            {
                HttpStatusCode.OK => "Container already existed...",
                HttpStatusCode.Created => $"Container {_containerId} was created successfully",
                _ => $"An error occured when attempting to create database. Http Status was {containerResponse.StatusCode}"
            };

            _logger.LogDebug(output);

            ContainerProperties properties = containerResponse;
            _logger.LogDebug($"Container resource id: {properties.Id} and last modified: {properties.LastModified}");

            return containerResponse;
        }



        public async Task CreateQuoteRecord(IMessage message)
        {
            try
            {
                QuotedCosmosRecord record = new(message);
                using var payloadStream = CosmosDbService.ToStream<QuotedCosmosRecord>(record);
                var response = await _container!.CreateItemAsync(record, new PartitionKey(message.Author.Id));

                _logger.LogDebug($"Item created");
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        private static Stream ToStream<T>(T input)
        {
            MemoryStream streamPayload = new MemoryStream();
            JsonSerializer.Serialize(streamPayload, input);

            streamPayload.Position = 0;
            return streamPayload;
        }

    }
}