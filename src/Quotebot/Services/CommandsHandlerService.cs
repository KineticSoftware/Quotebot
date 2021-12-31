using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Quotebot.Services
{
    internal class CommandsHandlerService
    {
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;

        public CommandsHandlerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            _commandService = _serviceProvider.GetRequiredService<CommandService>();
            _client = _serviceProvider.GetRequiredService<DiscordSocketClient>();

            _commandService.CommandExecuted += CommandExecutedAsync;
            _client.MessageReceived += MessageReceivedAsync;
        }

        public async Task InitializeAsync()
        {
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        public async Task MessageReceivedAsync(SocketMessage rawMessage)
        {
            var argPosition = 0;

            switch (rawMessage)
            {
                case SocketUserMessage message
                    when !message.HasCharPrefix('!', ref argPosition) :
                    return;

                case SocketUserMessage message
                    when message.Source != MessageSource.User:
                    return;

                case SocketUserMessage message:
                    var context = new SocketCommandContext(_client, message);
                    await _commandService.ExecuteAsync(context, argPosition, _serviceProvider);
                    return;

                case SocketMessage:
                    return;
            }
        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }

}
