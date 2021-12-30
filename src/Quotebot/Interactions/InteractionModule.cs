using Discord;
using Discord.Interactions;
using Quotebot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Interactions
{
    public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly InteractionHandlersService _handlerService;

        public InteractionService InteractionService { get; set; }

        public InteractionModule(InteractionHandlersService handlerService)
        {
            _handlerService = handlerService;
        }

        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("ping", "Recieve a pong")]
        // By setting the DefaultPermission to false, you can disable the command by default. No one can use the command until you give them permission
        [DefaultPermission(true)]
        public async Task Ping()
        {
            await RespondAsync("pong");
        }

        // You can use a number of parameter types in you Slash Command handlers (string, int, double, bool, IUser, IChannel, IMentionable, IRole, Enums) by default. Optionally,
        // you can implement your own TypeConverters to support a wider range of parameter types. For more information, refer to the library documentation.
        // Optional method parameters(parameters with a default value) also will be displayed as optional on Discord.

        // [Summary] lets you customize the name and the description of a parameter
        [SlashCommand("echo", "Repeat the input")]
        public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        {
            await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        }

        // [Group] will create a command group. [SlashCommand]s and [ComponentInteraction]s will be registered with the group prefix
        [Group("test_group", "This is a command group")]
        public class GroupExample : InteractionModuleBase<SocketInteractionContext>
        {
            // You can create command choices either by using the [Choice] attribute or by creating an enum. Every enum with 25 or less values will be registered as a multiple
            // choice option
            [SlashCommand("choice_example", "Enums create choices")]
            public async Task ChoiceExample(ExampleEnum input)
            {
                await RespondAsync(input.ToString());
            }
        }

        // User Commands can only have one parameter, which must be a type of SocketUser
        [UserCommand("SayHello")]
        public async Task SayHello(IUser user)
        {
            await RespondAsync($"Hello, {user.Mention}");
        }


        // Use [ComponentInteraction] to handle message component interactions. Message component interaction with the matching customId will be executed.
        // Alternatively, you can create a wild card pattern using the '*' character. Interaction Service will perform a lazy regex search and capture the matching strings.
        // You can then access these capture groups from the method parameters, in the order they were captured. Using the wild card pattern, you can cherry pick component interactions.
        [ComponentInteraction("musicSelect:*,*")]
        public async Task ButtonPress(string id, string name)
        {
            // ...
            await RespondAsync($"Playing song: {name}/{id}");
        }
    }
}
