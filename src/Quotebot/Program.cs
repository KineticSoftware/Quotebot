using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quotebot.Services;

public class Program
{

    //static void Main(string[] args) => new Program().MainAsync(args).GetAwaiter().GetResult();

    public static async Task Main(string[] args)
    {
        using var services = BuildServiceProvider();

        IConfiguration configuration = services.GetRequiredService<IConfiguration>();
        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();
        InteractionService interactionService = services.GetRequiredService<InteractionService>();


        async Task LogMessage(LogMessage log)
        {
            Console.WriteLine(log.Message);
            await Task.CompletedTask;
        }

        client.Log += LogMessage;
        interactionService.Log += LogMessage;

        client.Ready += async () =>
        {
//#if DEBUG
            await interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(configuration["GuildId"]), true);
//#else
            // await interactionService.RegisterCommandsGloballyAsync(true);
//#endif

            Console.WriteLine("Bot is connected!");
        };
#if Release
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
        string discordToken = (await keyVault.GetSecretAsync(keyUrl)).Value;
#else
        string discordToken = configuration["DiscordToken"];
#endif

        //await services.GetRequiredService<CommandHandlersService>().InitializeAsync();
        await services.GetRequiredService<InteractionHandlersService>().InitializeAsync();


        await client.LoginAsync(TokenType.Bot, discordToken);
        await client.StartAsync();

        Console.ReadLine();

        await client.StopAsync();
    }

    private ServiceProvider BuildServiceProvider()
    {
        IConfigurationBuilder builder = new ConfigurationBuilder()
                                    .AddEnvironmentVariables();
        IConfiguration configuration = builder.Build();

        return new ServiceCollection()
            .AddSingleton(configuration)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton(serviceProvider => new InteractionService(serviceProvider.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlersService>()
            .AddSingleton<InteractionHandlersService>()
            .BuildServiceProvider();
    }
}