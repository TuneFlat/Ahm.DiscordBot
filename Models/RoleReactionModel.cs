using Discord;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Ahm.DiscordBot.Models
{
    // Without the converter, an error will be thrown due to the Emojis.
    // TODO: look into using Emoji properites like id instead of the object. 
    public class RoleReactionModel
    {
        public ulong DiscordRoleId { get; set; }
        public ulong ReactMessageId { get; set; }
        public Emoji Emoji { get; set; }
        public Emote Emote { get; set; }
    }
}
