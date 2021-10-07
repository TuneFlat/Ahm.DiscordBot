using Ahm.DiscordBot.TypeConverters;
using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    // Without the converter, an error will be thrown due to the Emojis.
    // TODO: look into using Emoji properites like id instead of the object.
    [JsonConverter(typeof(SavedIdsConverter))]
    public class SavedIdsModel
    {
        public LFGRoleProperties lfgRoleProperties { get;set; }
        public PronounRoleProperties pronounRoleProperties { get; set; }

        
        public Dictionary<Emoji, ulong> AllEmojis => lfgRoleProperties.LFGEmojis
            .Union(pronounRoleProperties.PronounEmojis).ToDictionary(pair => pair.Key, pair => pair.Value);

        public List<ulong> ListOfMessageIds => new List<ulong> { 
            lfgRoleProperties.LFGRoleMessageId,
            pronounRoleProperties.PronounRoleMessageId
        };
    }

    public class LFGRoleProperties
    {
        public ulong LFGRoleMessageId { get; set; }
        public Dictionary<Emoji, ulong> LFGEmojis { get; set; }
    }
    
    public class PronounRoleProperties
    {
        public ulong PronounRoleMessageId { get; set; }
        public Dictionary<Emoji, ulong> PronounEmojis { get; set; }
    }
    
}
