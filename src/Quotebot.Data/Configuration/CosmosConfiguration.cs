using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotebot.Data
{
    internal class CosmosConfiguration
    {
        internal const string ConfigurationSectionName = "CosmosDb";

        public string Url { get; set; } = string.Empty;
        public string Authorization { get; set; } = string.Empty;
        public bool AlwaysRebuildContainer { get; set; }
    }
}
