using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Quotebot.Services;
using System.Configuration;

public class Program
{
    private DiscordSocketClient? _client;

    static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

    public async Task MainAsync(string[] args)
    {
        using var services = BuildServiceProvider();


        _client = services.GetService<DiscordSocketClient>();      
        

        _client.Log += async (logMessage) =>
       {
           Console.WriteLine(logMessage.Message);
           await Task.CompletedTask;
       };

        _client.Ready += async () => {
            Console.WriteLine("Bot is connected!");
            await Task.CompletedTask;
        };

        string apiClient = ConfigurationManager.AppSettings["ApiClientId"] ?? string.Empty;
        string apiSecret = ConfigurationManager.AppSettings["ApiSecret"] ?? string.Empty;
        string keyUrl = ConfigurationManager.AppSettings["TokenSecretUri"] ?? string.Empty;

        if(string.IsNullOrWhiteSpace(apiClient) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(keyUrl)
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
        Console.WriteLine(token);

        
        await services.GetRequiredService<CommandHandlersService>().InitializeAsync();

        Console.ReadLine();

        await _client.StopAsync();
    }

    private ServiceProvider BuildServiceProvider()
    {
        return new ServiceCollection()
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlersService>()
            .BuildServiceProvider();
    }
}