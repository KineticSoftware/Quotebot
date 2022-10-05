using Discord.Interactions;

namespace Quotebot;

internal class Bot
{
    private readonly ILogger<Bot> _logger;
    private readonly DiscordConfiguration _configuration;
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly CommandsHandlerService _commandsHandlerService;
    private readonly InteractionsHandlerService _interactionsHandlerService;
    private readonly ItsWednesdayMyDudesService _itsWednesdayMyDudesService;

    public Bot(
        ILogger<Bot> logger,
        DiscordConfiguration configuration,
        DiscordSocketClient client,
        InteractionService interactionService,
        CommandsHandlerService commandsHandlerService,
        InteractionsHandlerService interactionsHandlerService,
        ItsWednesdayMyDudesService itsWednesdayMyDudesService)
    {
        _logger = logger;
        _configuration = configuration;
        _client = client;
        _interactionService = interactionService;
        _commandsHandlerService = commandsHandlerService;
        _interactionsHandlerService = interactionsHandlerService;
        _itsWednesdayMyDudesService = itsWednesdayMyDudesService;

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
        _itsWednesdayMyDudesService.Initialize();
        await Task.WhenAll(_commandsHandlerService.InitializeAsync(),
            _interactionsHandlerService.InitializeAsync());
        await _client.LoginAsync(TokenType.Bot, discordToken);
        await _client.StartAsync();
    }

    private async Task LogMessage(LogMessage log)
    {
        _logger.LogDebug(log.Message);
        await Task.CompletedTask;
    }

    private async Task OnReady()
    {
        await _interactionService.RegisterCommandsToGuildAsync(_configuration.GuildId);
        _logger.LogInformation("Bot is connected!");
    }

    public async Task OnShutdown()
    {
        _logger.LogInformation("Disconnecting...");
        await _client.LogoutAsync();
    }
}
