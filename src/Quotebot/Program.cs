using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Quotebot.Data;
using Quotebot.Services;

namespace Quotebot
{
    public class Program
    {
        private static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
        return false;
#endif
            }
        }

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
            //if(IsDebug)
            //{
                await interactionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(configuration["GuildId"]), true);

            //}
            //else
            //{
            //    await interactionService.RegisterCommandsGloballyAsync(true);
            //}

            Console.WriteLine("Bot is connected!");
            };
            string discordToken = string.Empty;
            if (IsDebug)
            {
                discordToken = configuration["DiscordToken"];
            }
            else
            {
                string apiClient = configuration["ApiClientId"];
                string apiSecret = configuration["ApiSecret"];
                string keyUrl = configuration["TokenSecretUri"];

                if (string.IsNullOrWhiteSpace(apiClient) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(keyUrl))
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
                discordToken = (await keyVault.GetSecretAsync(keyUrl)).Value;
            }

            await services.GetRequiredService<IDataService>().Initialize();
            await services.GetRequiredService<CommandsHandlerService>().InitializeAsync();
            await services.GetRequiredService<InteractionsHandlerService>().InitializeAsync();


            await client.LoginAsync(TokenType.Bot, discordToken);
            await client.StartAsync();

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += async (s, e) =>
            {
                Console.WriteLine("Disconnecting...");
                await client.LogoutAsync();
                cancellationTokenSource.Cancel();
                e.Cancel = true;
            };

            try
            {
                await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static ServiceProvider BuildServiceProvider()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                                        .AddEnvironmentVariables();
            IConfiguration configuration = builder.Build();

            return new ServiceCollection()
                .AddSingleton(configuration)
                .RegisterDiscordServices()
                .RegisterCosmosDb()
                .BuildServiceProvider();
        }
    }
}