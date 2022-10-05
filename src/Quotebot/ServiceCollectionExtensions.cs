using Discord.Commands;
using Discord.Interactions;
using Quotebot.Interactions;

namespace Quotebot;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection RegisterServices(this IServiceCollection serviceCollection, IConfiguration parentConfiguration, CancellationTokenSource cancellationTokenSource)
    {
        DiscordConfiguration discordConfiguration = parentConfiguration.GetRequiredSection(DiscordConfiguration.ConfigurationSectionName).Get<DiscordConfiguration>();
        YoutubeConfiguration youtubeConfiguration = parentConfiguration
            .GetRequiredSection(YoutubeConfiguration.ConfigurationSectionName).Get<YoutubeConfiguration>();
        return serviceCollection
            .AddSingleton(discordConfiguration)
            .AddSingleton(_ => new DiscordSocketClient(
                new DiscordSocketConfig
                {
                    LogLevel = LogSeverity.Debug,
                    MessageCacheSize = 50,
                    GatewayIntents = GatewayIntents.All
                }))
            .AddSingleton(serviceProvider =>
                new InteractionService(serviceProvider.GetRequiredService<DiscordSocketClient>()))
            .AddSingleton(_ => new CommandService(new CommandServiceConfig
            {
                LogLevel = LogSeverity.Debug,
                ThrowOnError = true
            }))
            .AddSingleton<CommandsHandlerService>()
            .AddSingleton<InteractionsHandlerService>()
            .AddSingleton<ItsWednesdayMyDudesService>(serviceProvider =>
                new(serviceProvider.GetRequiredService<DiscordSocketClient>(), discordConfiguration, youtubeConfiguration, serviceProvider.GetRequiredService<ILogger<ItsWednesdayMyDudesService>>(), cancellationTokenSource))
            .AddSingleton<EmojiReactionHandler>();
    }
}
