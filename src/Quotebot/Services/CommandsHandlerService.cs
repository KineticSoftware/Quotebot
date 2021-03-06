using Discord.Commands;
using System.Reflection;
using System.Text;
using Quotebot.Domain.Validators;

namespace Quotebot.Services;

public class CommandsHandlerService
{
    private readonly CommandService _commandService;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataService _dataService;

    public CommandsHandlerService(IServiceProvider serviceProvider, IDataService dataService)
    {
        _serviceProvider = serviceProvider;
        _dataService = dataService;

        _commandService = _serviceProvider.GetRequiredService<CommandService>();
        _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

        _commandService.CommandExecuted += CommandExecutedAsync;
        _client.MessageReceived += MessageReceivedAsync;
        _client.ReactionAdded += ReactionAddedAsync;
    }

    public async Task InitializeAsync()
    {
        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
    }

    private async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        var argPosition = 0;

        switch (rawMessage)
        {
            case SocketUserMessage { Source: MessageSource.User } message when message.HasCharPrefix('!', ref argPosition):
                var context = new SocketCommandContext(_client, message);
                await _commandService.ExecuteAsync(context, argPosition, _serviceProvider);
                break;
            default: 
                await Task.CompletedTask;
                break;
        };
    }

    private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedUserMessage, Cacheable<IMessageChannel, ulong> cachedChannelMessage, SocketReaction reaction)
    {
        var userMessage = await cachedUserMessage.GetOrDownloadAsync();
        if (userMessage is { Author.IsBot: true } )
            return;

        var emotedValue = userMessage.Reactions.TryGetValue(BotEmotes.QuotedEmote(), out var emoteMetadata);
        if (!emotedValue || emoteMetadata.ReactionCount > 1 || reaction.User.Value.IsBot)
            return;

        var channelMessage = await cachedChannelMessage.GetOrDownloadAsync();
        if (channelMessage is not IGuildChannel)
            return;

        if (reaction.User.GetValueOrDefault() is not SocketGuildUser socketGuildUser)
            return;

        var validator = userMessage.Validate();
        if(!validator.IsValid)
            return;

        var quote = new Quoted(userMessage);

        var result = await _dataService.TryCreateQuoteRecord(quote);
        if (!result)
        {
            await userMessage.ReplyAsync($"This quote was already added.");
            return;
        }
        
        var response = new StringBuilder()
            .AppendLine($"_{socketGuildUser.Nickname} quoted via_ {BotEmotes.QuotedRaw}")
            .AppendLine($"> *{quote.Author?.Nickname ?? quote.Author?.Username} : {userMessage.Content}*");

        await userMessage.ReplyAsync($"{response}");
    }

    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified || result.IsSuccess)
            return;

        await context.Channel.SendMessageAsync($"Error: {result}");
    }
}
