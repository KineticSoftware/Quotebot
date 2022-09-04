using Azure.Identity;
using Microsoft.Extensions.Hosting;


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
                Uri keyUrl = new Uri(configuration["TokenSecretUri"]);
                configBuilder.AddAzureKeyVault(keyUrl, new ClientSecretCredential("26789f3b-4e12-4923-81d4-db4519203698", apiClient, apiSecret));
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
