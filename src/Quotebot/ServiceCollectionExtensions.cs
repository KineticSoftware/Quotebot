using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Quotebot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection RegisterDiscordNet(this IServiceCollection serviceCollection)
        {
            return serviceCollection.AddSingleton(serviceProvider => new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = 50,
                }))
                .AddSingleton(serviceProvider => new InteractionService(serviceProvider.GetRequiredService<DiscordSocketClient>()))
                .AddSingleton(serviceProvider => new CommandService(new CommandServiceConfig
                {
                    // Again, log level:
                    LogLevel = LogSeverity.Debug,
                    ThrowOnError = true
                }))
                .AddSingleton<CommandsHandlerService>()
                .AddSingleton<InteractionsHandlerService>();

        }
    }
}
