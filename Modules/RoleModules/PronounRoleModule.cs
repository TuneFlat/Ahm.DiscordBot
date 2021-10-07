using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Ahm.DiscordBot.Modules.RoleModules
{
    class PronounRoleModule : ModuleBase<SocketCommandContext>
    {
        private ulong _pronounMessageId;

        readonly Dictionary<Emoji, ulong> pronounEmojis = new Dictionary<Emoji, ulong>(){
            { new Emoji("1️⃣"), 884295099318669313},
            { new Emoji("2️⃣"), 884295182068101170},
            { new Emoji("3️⃣"), 884295223432314950}
        };

        [Command("add pronoun reactions")]
        public async Task ReactPronounRoles([Remainder] ulong messageId)
        {
            await Context.Message.DeleteAsync();

            var messageForRoles = (IUserMessage)await Context.Channel.GetMessageAsync(messageId);
            await messageForRoles.AddReactionsAsync(pronounEmojis.Select(x => x.Key).ToArray());
        }
    }
}
