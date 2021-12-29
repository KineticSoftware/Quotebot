using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Quotebot.Services;

public class Program
{

    static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

    public async Task MainAsync(string[] args)
    {
        using var services = BuildServiceProvider();

        IConfigurationRoot configuration = services.GetRequiredService<IConfigurationRoot>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();      
        

        client.Log += async (logMessage) =>
       {
           Console.WriteLine(logMessage.Message);
           await Task.CompletedTask;
       };

        client.Ready += async () => {
            Console.WriteLine("Bot is connected!");
            await Task.CompletedTask;
        };

        string apiClient = configuration["ApiClientId"];
        string apiSecret = configuration["ApiSecret"];
        string keyUrl = configuration["TokenSecretUri"];
         
        if(string.IsNullOrWhiteSpace(apiClient) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(keyUrl))
        {
            Console.WriteLine("Unable to determine Azure Credentials. Exiting...");
            return;
        }

        var keyVault = new KeyVaultClient(async (string authority, string resource, string scope) =>
        {
            var authContext = new AuthenticationContext(authority);
            var credential = new ClientCredential(apiClient, apiSecret);
            var token = await authContext.AcquireTokenAsync(resource, credential);

            return token.AccessToken;
        });

        // Get the API key out of the vault
        string token = (await keyVault.GetSecretAsync(keyUrl)).Value;
        
        await services.GetRequiredService<CommandHandlersService>().InitializeAsync();

        await client.StartAsync();

        Console.ReadLine();

        await client.StopAsync();
    }

    private ServiceProvider BuildServiceProvider()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                                    .AddEnvironmentVariables();
        IConfigurationRoot configuration = builder.Build();

        return new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlersService>()
            .BuildServiceProvider();
    }
}