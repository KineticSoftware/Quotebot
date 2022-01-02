using Discord.Interactions;
using Microsoft.Azure.KeyVault;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Quotebot.Data;
using Quotebot.Services;

namespace Quotebot
{
    public class Bot
    {

#if DEBUG
        private readonly bool IsDebug = true;
#else
        private readonly bool IsDebug = false;
#endif

        private readonly DiscordSocketClient Client;
        private readonly IConfiguration Configuration;
        private readonly InteractionService InteractionService;
        private readonly IDataService DataService;
        private readonly CommandsHandlerService CommandsHandlerService;
        private readonly InteractionsHandlerService InteractionsHandlerService;

        public Bot(IConfiguration configuration,
            DiscordSocketClient client,
            InteractionService interactionService,
            IDataService dataService,
            CommandsHandlerService commandsHandlerService,
            InteractionsHandlerService interactionsHandlerService)
        {
            Configuration = configuration;
            Client = client;
            InteractionService = interactionService;
            CommandsHandlerService = commandsHandlerService;
            InteractionsHandlerService = interactionsHandlerService;
            DataService = dataService;
            RegisterEventHandlers();
        }

        private void RegisterEventHandlers()
        {
            Client.Log += LogMessage;
            Client.Ready += OnReady;
            InteractionService.Log += LogMessage;
        }

        public async Task Connect()
        {
            var discordToken = await GetDiscordToken();
            await Task.WhenAll(CommandsHandlerService.InitializeAsync(),
                InteractionsHandlerService.InitializeAsync(),
                DataService.Initialize());
            await Client.LoginAsync(TokenType.Bot, discordToken);
            await Client.StartAsync();
        }

        private async Task<string> GetDiscordToken() =>
            IsDebug
                ? Configuration["DiscordToken"]
                : await GetDiscordTokenAzure();

        private async Task<string> GetDiscordTokenAzure()
        {
            string apiClient = Configuration["ApiClientId"];
            string apiSecret = Configuration["ApiSecret"];
            string keyUrl = Configuration["TokenSecretUri"];

            if (string.IsNullOrWhiteSpace(apiClient) || string.IsNullOrWhiteSpace(apiSecret) || string.IsNullOrWhiteSpace(keyUrl))
            {
                Console.WriteLine("Unable to determine Azure Credentials. Exiting...");
                return string.Empty;
            }

            var keyVault = new KeyVaultClient(async (string authority, string resource, string scope) =>
            {
                var authContext = new AuthenticationContext(authority);
                var credential = new ClientCredential(apiClient, apiSecret);
                var token = await authContext.AcquireTokenAsync(resource, credential);

                return token.AccessToken;
            });
            // Get the API key out of the vault
            return (await keyVault.GetSecretAsync(keyUrl)).Value;
        }

        public static async Task LogMessage(LogMessage log) =>
            await Task.Run(() => Console.WriteLine(log.Message));

        public async Task OnReady()
        {
            //if(IsDebug)
            //{
            await InteractionService.RegisterCommandsToGuildAsync(Convert.ToUInt64(Configuration["GuildId"]), true);
            //}
            //else
            //{
            //    await InteractionService.RegisterCommandsGloballyAsync(true);
            //}
            Console.WriteLine("Bot is connected!");
        }

        public async Task OnShutdown()
        {
            Console.WriteLine("Disconnecting...");
            await Client.LogoutAsync();
        }
    }
}
