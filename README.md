# Quotebot
CONerd Quotebot is this idea that any message spoken in a Discord server could be recorded to a quote database. Specific quotes could then be retrieved in the future, or even searched.  This is recreating old quotebot functionality some of you may remember from IRC.

## Checklist
- [x] Create the bot in Discord
- [x] reate an Azure Webjob to run the bot
- [x] Integrate Azure Secrets Vault integration
- [x] Write some baseline code for the bot to join the server
- [x] Write a simple baseline interaction with the bot responding to a user
- [x] Build CI/CD pipeline
- [x] Determine the database technology (CosmosDb)
- [x] Add database service implementation
  - [x] Determine table schema (Date/Time, messageID (primary key), username, message, etc.)
- [x] Add `!quote add`
  - [x] Accessed by right clicking on a message (context menu) and then via `Apps` menu item. 
  - [x] While replying to message `/quote add`
  - [x] When using the `:quoted:` empoji
  - [x] Data storage implemenation
- [x] Add `Count of Quotes` as a user action (right click on a User)
- [x] Make quotebot add an emoji to quoted text
- [x] Add `!quote find <text>`
- [x] **Top Requested Feature** - Display a random quote
- [ ] Should we set [ephemeral](https://github.com/discord-net/Discord.Net/blob/dev/src/Discord.Net.Core/Entities/Interactions/IDiscordInteraction.cs#L58)?
  - [ ] What about the other options? e..g allowedMentions 
- [ ] Allow embeds or attachments??
- [ ] NTH Add `!quote messageid`
- [ ] Search quotes by username
- [ ] Autosuggest find quote by text or username
- [ ] Add Application Insights or another type of application telemetry.
- [ ] (ﾉ◕ヮ◕)ﾉ*:･ﾟ✧
