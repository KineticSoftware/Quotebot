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
        public async Task FindQuote(string text)
        {
            StringBuilder stringBuilder = new();
            var results = await _dataService.FindByQuote(text);
            if (results == Enumerable.Empty<Quoted>())
            {
                await ReplyAsync($"No quotes found containg the text {text}");
                return;
            }

            foreach (var quote in results)
            {
                stringBuilder.AppendLine($"{quote.Content}")
                    .AppendLine($"*by {quote.Author?.Username} on {quote.CreatedAt.ToString("d")}*");
            }

            await RespondAsync(stringBuilder.ToString());
        }

        // [Summary] lets you customize the name and the description of a parameter
        //[SlashCommand("addquote", "Repeat the input")]
        //public async Task Echo(string echo, [Summary(description: "mention the user")] bool mention = false)
        //{
        //    await RespondAsync(echo + (mention ? Context.User.Mention : string.Empty));
        //}
    }
}
