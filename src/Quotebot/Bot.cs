using Discord.Interactions;
using Microsoft.Extensions.Hosting;
using Quotebot.Configuration;
using Quotebot.Data;
using Quotebot.Services;

namespace Quotebot;

internal class Bot
{
    private readonly ILogger<Bot> _logger;
    private readonly IHostEnvironment _environment;
    private readonly BotConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly CommandsHandlerService _commandsHandlerService;
    private readonly InteractionsHandlerService _interactionsHandlerService;

    public Bot(
        ILogger<Bot> logger,
        IHostEnvironment environment,
        BotConfiguration configuration,
        DiscordSocketClient client,
        InteractionService interactionService,
        IDataService dataService,
        CommandsHandlerService commandsHandlerService,
        InteractionsHandlerService interactionsHandlerService)
    {
        _logger = logger;
        _configuration = configuration;
        _environment = environment;
        _client = client;
        _interactionService = interactionService;
        _commandsHandlerService = commandsHandlerService;
        _interactionsHandlerService = interactionsHandlerService;
        RegisterEventHandlers();
    }

    private void RegisterEventHandlers()
    {
        _client.Log += LogMessage;
        _client.Ready += OnReady;
        _interactionService.Log += LogMessage;
    }

    public async Task Connect()
    {
        var discordToken = _configuration.Token;
        await Task.WhenAll(_commandsHandlerService.InitializeAsync(),
            _interactionsHandlerService.InitializeAsync());
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();
    }

    public async Task LogMessage(LogMessage log)
    {
        _logger.LogDebug(log.Message);
        await Task.CompletedTask;
    }

    public async Task OnReady()
    {
        await _interactionService.RegisterCommandsToGuildAsync(_configuration.GuildId, true);
        _logger.LogInformation("Bot is connected!");
    }

    public async Task OnShutdown()
    {
        _logger.LogInformation("Disconnecting...");
        await _client.LogoutAsync();
    }
}
