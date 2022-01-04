using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Net;

namespace Quotebot.Data
{
    public class CosmosDbService : IDataService
    {
        private readonly CosmosClient _cosmosClient;

        //// The container we will create.
        //private Container container;

        //// The name of the database and container we will create
        private readonly string _databaseId = "Quotebot";
        private readonly string _containerId = "Quotes";
        private readonly string _partitionKeyPath = "/users/id";

        public CosmosDbService(CosmosClient cosmosClient, ILogger<CosmosDbService> logger)
        {
            _cosmosClient = cosmosClient;
            _logger = logger;
        }

        public async Task Initialize()
        {
            try
            {
                var database = await InitializeDatabase();
                await InitializeContainer(database);
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
    }
}