using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Quotebot.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterCosmosDb(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddSingleton(serviceProvider =>
                {
                    var configuration = serviceProvider.GetRequiredService<IConfiguration>();

                    string endpoint = configuration[Constants.CosmosUrlConfigurationKey];
                    if (string.IsNullOrWhiteSpace(endpoint))
                    {
                        throw new ArgumentNullException($"Please specify a valid {Constants.CosmosUrlConfigurationKey} in environment variables");
                    }

                    string authKey = configuration[Constants.CosmosAuthorizationConfigurationKey];
                    if (string.IsNullOrWhiteSpace(authKey))
                    {
                        throw new ArgumentException($"Please specify a valid {Constants.CosmosAuthorizationConfigurationKey} in the appSettings.json");
                    }

                    return new CosmosClient(configuration[Constants.CosmosUrlConfigurationKey], configuration[Constants.CosmosAuthorizationConfigurationKey]);
                })
                .AddSingleton<IDataService, CosmosDbService>();
        }
    }
}
