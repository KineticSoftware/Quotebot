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

        public CosmosDbService(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
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
                Console.WriteLine(ex);
            }
        }

        private async Task<Database> InitializeDatabase()
        {
            DatabaseResponse databaseResponse = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_databaseId);

            string output = databaseResponse.StatusCode switch
            {
                HttpStatusCode.OK => "Database already existed...",
                HttpStatusCode.Created => $"Database {_databaseId} was created successfully",
                _ => $"An error occured when attempting to create database. Http Status was {databaseResponse.StatusCode}"
            };

            Console.WriteLine(output);

            // The response from Azure Cosmos
            DatabaseProperties properties = databaseResponse;

            Console.WriteLine($"Database resource id: {properties.Id} and last modified: {properties.LastModified}");

            return databaseResponse;
        }
            
        private async Task<Container> InitializeContainer(Database database)
        {
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

            Console.WriteLine(output);

            ContainerProperties properties = containerResponse;
            Console.WriteLine($"Container resource id: {properties.Id} and last modified: {properties.LastModified}");

            return containerResponse;
        }
    }
}