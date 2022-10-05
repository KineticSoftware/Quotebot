using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Configuration
{
    internal class YoutubeConfiguration
    {
        public const string ConfigurationSectionName = "Youtube";

        public string ApiKey { get; init; } = string.Empty;
    }
}
