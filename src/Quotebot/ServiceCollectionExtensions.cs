using Discord.Commands;
using Discord.Interactions;
using Quotebot.Interactions.EmoteReaction;

namespace Quotebot;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection RegisterServices(this IServiceCollection serviceCollection, IConfiguration parentConfiguration, CancellationTokenSource cancellationTokenSource)
    {
        DiscordConfiguration discordConfiguration = parentConfiguration.GetRequiredSection(DiscordConfiguration.ConfigurationSectionName).Get<DiscordConfiguration>() 
                                                        ?? throw new ArgumentException($"{DiscordConfiguration.ConfigurationSectionName} configuration section was not specified.");
        
        YoutubeConfiguration youtubeConfiguration = parentConfiguration.GetRequiredSection(YoutubeConfiguration.ConfigurationSectionName).Get<YoutubeConfiguration>() 
                                                        ?? throw new ArgumentException($"{YoutubeConfiguration.ConfigurationSectionName} configuration section was not specified.");

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
            .AddSingleton<QuotedEmoteReactionHandler>();
    }
}
