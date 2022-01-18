using Discord.Interactions;
using System.Reflection;

namespace Quotebot.Services;

public class InteractionsHandlerService
{
    private readonly DiscordSocketClient _client;
    private readonly InteractionService _interactionService;
    private readonly IServiceProvider _serviceProvider;

    public InteractionsHandlerService(DiscordSocketClient client, InteractionService interactionService, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _client = client;
        _interactionService = interactionService;
    }

    public async Task InitializeAsync()
    {
        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);

        _client.InteractionCreated += InteractionCreated;
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        _interactionService.ContextCommandExecuted += ContextCommandExecuted;
        _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;
    }

    private async Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                case InteractionCommandError.UnknownCommand:
                case InteractionCommandError.BadArgs:
                case InteractionCommandError.Exception:
                case InteractionCommandError.Unsuccessful:
                    Console.WriteLine($"{result.Error} {result.ErrorReason}");
                    break;
                default:
                    break;
            }
        }
        await Task.CompletedTask;
    }

    private async Task ContextCommandExecuted(ContextCommandInfo contextInfo, IInteractionContext context, IResult result)
    {
        // Message Commands
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                case InteractionCommandError.UnknownCommand:
                case InteractionCommandError.BadArgs:
                case InteractionCommandError.Exception:
                case InteractionCommandError.Unsuccessful:
                    Console.WriteLine($"{result.Error} {result.ErrorReason}");
                    break;
                default:
                    break;
            }
        }

        await Task.CompletedTask;
    }

    private async Task SlashCommandExecuted(SlashCommandInfo slashInfo, IInteractionContext context, IResult result)
    {
        if (!result.IsSuccess)
        {
            switch (result.Error)
            {
                case InteractionCommandError.UnmetPrecondition:
                case InteractionCommandError.UnknownCommand:
                case InteractionCommandError.BadArgs:
                case InteractionCommandError.Exception:
                case InteractionCommandError.Unsuccessful:
                    Console.WriteLine($"{result.Error} {result.ErrorReason}");
                    break;
                default:
                    break;
            }
        }

        await Task.CompletedTask;
    }

    private async Task InteractionCreated(SocketInteraction socketInteraction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(_client, socketInteraction);
            await _interactionService.ExecuteCommandAsync(ctx, _serviceProvider);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);

            // If a Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (socketInteraction.Type is InteractionType.ApplicationCommand)
                await socketInteraction.GetOriginalResponseAsync()
                    .ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}
