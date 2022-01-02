global using Discord;
global using Microsoft.Extensions.Configuration;
global using Discord.WebSocket;
using Discord.Commands;
using Discord.Interactions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quotebot;
using Quotebot.Data;
using Quotebot.Services;

try
{
    var host = new HostBuilder()
        .ConfigureAppConfiguration((hostContext, configBuilder) =>
        {
            configBuilder.AddEnvironmentVariables();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services
                .AddSingleton(serviceProvider => new DiscordSocketClient(new DiscordSocketConfig
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
                .AddSingleton<InteractionsHandlerService>()
                .RegisterCosmosDb(hostContext.Configuration)
                .AddSingleton<Bot>();
        })
        .Build();
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    var runningHost = host.RunAsync(cancellationTokenSource.Token);
    var bot = host.Services.GetService<Bot>();
    if (bot == null) throw new ApplicationException("Could not resolve bot service.");
    Console.CancelKeyPress += async (s, e) =>
    {
        await bot.OnShutdown();
        cancellationTokenSource.Cancel();
        e.Cancel = true;
    };
    await bot.Connect();
    await runningHost;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
}
