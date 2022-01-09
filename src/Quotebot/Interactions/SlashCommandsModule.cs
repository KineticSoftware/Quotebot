using Discord.Commands;
using Discord.Interactions;
using Quotebot.Data;
using Quotebot.Data.Entities;
using System.Text;

namespace Quotebot.Interactions
{
    public class SlashCommandsModule : InteractionModuleBase<SocketInteractionContext>
    {
        IDataService _dataService;
        public SlashCommandsModule(IDataService dataService)
        {
            _dataService = dataService;
        }

        // Slash Commands are declared using the [SlashCommand], you need to provide a name and a description, both following the Discord guidelines
        [SlashCommand("findquote", "Finds a quote")]
        // By setting the DefaultPermission to false, you can disable the command by default. No one can use the command until you give them permission
        public async Task FindQuote(string text, int limit = 5)
        {
            await DeferAsync();
            
            var results = await _dataService.FindByQuote(text, Context.Channel.Id, limit);
            
            await FollowupAsync(results);
        }

        // [Summary] lets you customize the name and the description of a parameter
        //[SlashCommand("addquote", "Repeat the input")]
        //public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        //{
        //    await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        //}
    }
}
