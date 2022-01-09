﻿using Discord.Commands;
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

        [SlashCommand("findquote", "Finds a quote")]
        public async Task FindQuote(string text, int limit = 5)
        {
            await DeferAsync();
            
            var results = await _dataService.FindByQuote(text, Context.Channel.Id, limit);
            
            await FollowupAsync(results);
        }

        [SlashCommand("findserverquote", "Finds a quote across all channels in this server")]
        public async Task FindServerQuote(string text, int limit = 5)
        {
            await DeferAsync();

            var results = await _dataService.FindByQuoteInServer(text, limit);

            await FollowupAsync(results);
        }


    }
}
