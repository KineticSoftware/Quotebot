using Azure.Identity;
using Microsoft.Extensions.Hosting;

try
{
    CancellationTokenSource cancellationTokenSource = new();
    IHost host = new HostBuilder()
        .ConfigureDefaults(args)
        .ConfigureAppConfiguration((hostContext, configBuilder) =>
        {
            var configuration = configBuilder.Build();

            if (hostContext.HostingEnvironment.IsProduction())
            {
                string apiClient = configuration["ApiClientId"] ?? throw new ArgumentException("ApiClientId was not found.");
                string apiSecret = configuration["ApiSecret"] ?? throw new ArgumentException("ApiSecret was not found.");
                Uri keyUrl = new Uri(configuration["TokenSecretUri"] ?? throw new ArgumentException("TokenSecretUri was not found."));
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
                .RegisterServices(hostContext.Configuration, cancellationTokenSource)
                .RegisterCosmosDb(hostContext.Configuration)
                .AddSingleton<Bot>();
        })
        .Build();

    
    var runningHost = host.RunAsync(cancellationTokenSource.Token);
    var bot = host.Services.GetService<Bot>();
    
    if (bot == null) throw new ApplicationException("Could not resolve bot service.");
    Console.CancelKeyPress += async (_, e) =>
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
