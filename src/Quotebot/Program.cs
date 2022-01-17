global using Discord;
global using Microsoft.Extensions.Configuration;
global using Discord.WebSocket;
global using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quotebot;
using Quotebot.Data;


try
{
    var host = new HostBuilder()
        .ConfigureDefaults(args)
        .ConfigureAppConfiguration((hostContext, configBuilder) =>
        {
            var configuration = configBuilder.Build();

            if (hostContext.HostingEnvironment.IsProduction())
            {
                string apiClient = configuration["ApiClientId"];
                string apiSecret = configuration["ApiSecret"];
                string keyUrl = configuration["TokenSecretUri"];
                configBuilder.AddAzureKeyVault(keyUrl, apiClient, apiSecret);
            }
        })
        .ConfigureLogging(hostBuilder =>
        {
            hostBuilder.ClearProviders();
            hostBuilder.AddConsole();
        })
        .ConfigureServices((hostContext, services) =>
        {
            services
                .RegisterDiscordNet(hostContext.Configuration)
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
