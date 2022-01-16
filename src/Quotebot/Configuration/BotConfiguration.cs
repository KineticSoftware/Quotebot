using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Configuration
{
    internal class BotConfiguration
    {
        public const string ConfigurationSectionName = "Discord";

        public string Token { get; set; } = string.Empty;
        public ulong GuildId { get; set; }

    }
}
