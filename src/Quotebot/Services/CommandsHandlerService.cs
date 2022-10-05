using Discord.Commands;
using System.Reflection;
using Quotebot.Interactions;

namespace Quotebot.Services;

public class CommandsHandlerService
{
    private readonly CommandService _commandService;
    private readonly DiscordSocketClient _client;
    private readonly IServiceProvider _serviceProvider;
    private readonly EmojiReactionHandler _emojiReactionHandler;

    public CommandsHandlerService(IServiceProvider serviceProvider, EmojiReactionHandler emojiReactionHandler, CommandService commandService, DiscordSocketClient client)
    {
        _serviceProvider = serviceProvider;
        _emojiReactionHandler = emojiReactionHandler;

        _commandService = commandService;
        _client = client;

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
        }
    }

    private async Task ReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedUserMessage, Cacheable<IMessageChannel, ulong> cachedChannelMessage, SocketReaction reaction)
    {
        await _emojiReactionHandler.ReactionAddedAsync(cachedUserMessage, cachedChannelMessage, reaction);
    }

    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (!command.IsSpecified || result.IsSuccess)
            return;

        await context.Channel.SendMessageAsync($"Error: {result}");
    }
}
