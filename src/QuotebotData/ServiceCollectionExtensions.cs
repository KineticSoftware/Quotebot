using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Quotebot.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCosmosDb(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            string endpoint = configuration[Constants.CosmosUrlConfigurationKey];
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                Console.WriteLine($"Please specify a valid {Constants.CosmosUrlConfigurationKey} in environment variables");
                return serviceCollection;
            }

            string authKey = configuration[Constants.CosmosAuthorizationConfigurationKey];
            if (string.IsNullOrWhiteSpace(authKey))
            {
                Console.WriteLine($"Please specify a valid {Constants.CosmosAuthorizationConfigurationKey} in the appSettings.json");
                return serviceCollection;
            }

            return serviceCollection
                .AddSingleton(serviceProvider =>
                {
                    return new CosmosClient(configuration[Constants.CosmosUrlConfigurationKey], configuration[Constants.CosmosAuthorizationConfigurationKey]);
                })
                .AddSingleton<IDataService, CosmosDbService>();
        }
    }
}
