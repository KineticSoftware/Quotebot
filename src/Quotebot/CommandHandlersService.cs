using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Quotebot
{
    internal class CommandHandlersService
    {
        private readonly CommandService _commandService;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandlersService(IServiceProvider serviceProvider)
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
            if (!(rawMessage is SocketUserMessage message))
                return;

            if (message.Source != Discord.MessageSource.User)
                return;

            var argPosition = 0;
            if (!message.HasCharPrefix('!', ref argPosition))
                return;

            var context = new SocketCommandContext(_client, message);

            await _commandService.ExecuteAsync(context, argPosition, _serviceProvider);

        }

        public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified)
                return;

            if (result.IsSuccess)
                return;

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }

}
