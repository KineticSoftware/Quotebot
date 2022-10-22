using Discord.Commands;
using System.Reflection;
using Microsoft.Extensions.Hosting;

namespace Quotebot.Interactions.TextCommand;

[Group("mage")]
public class MageTextCommandModule : ModuleBase<SocketCommandContext>
{
    private readonly IHostEnvironment _hostEnvironment;

    public MageTextCommandModule(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }
    [Command("version")]
    [Summary("Gets the bot's current version")]
    public async Task SayVersion()
    {
        Version version = Assembly.GetEntryAssembly()?.GetName().Version ??
                          throw new Exception("Unable to determine entry assembly");
        await ReplyAsync($"Version: `{version.Major}.{version.Minor}.{version.Build:000#}.{version.Revision}`");
    }

    [Command("env")]
    [Summary("Gets the bot's current environment")]
    public async Task SayEnvironment()
    {
        await ReplyAsync($"Current Environment: `{_hostEnvironment.EnvironmentName}`");
    }
}