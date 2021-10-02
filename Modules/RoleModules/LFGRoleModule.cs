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
    public class LFGRoleModule : ModuleBase<SocketCommandContext>
    {
        #region LFG 

        private ulong _lfgMessageId;

        readonly Dictionary<Emoji, ulong> lfgEmojis = new Dictionary<Emoji, ulong>(){
            { new Emoji("🫐"),  884290611350491136 },
            { new Emoji("🍓"), 884290674684461066 },
            { new Emoji("😠"), 884290705944637501 }
        };

        [Command("ReactLFGRoles")]
        public async Task ReactLFGRoles()
        {
            await Context.Message.DeleteAsync();

            var lfgMessage = (RestUserMessage)await ReplyAsync(LFGMessageBuilder());
            await lfgMessage.AddReactionsAsync(lfgEmojis.Select(x => x.Key).ToArray());
            _lfgMessageId = lfgMessage.Id;

            Context.Client.ReactionAdded += UserReactedToLFG;
            Context.Client.ReactionRemoved += UserRemovedReactionToLFG;
        }


        private string LFGMessageBuilder()
        {
            var message = "LFG Roles\n";
            var pve = new Emoji("🫐") + " PVE\n";
            var pvp = new Emoji("🍓") + " PVP\n";
            var trials = new Emoji("😠") + " Trials\n";

            return message + pve + pvp + trials;
        }

        public async Task UserReactedToLFG(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            if (!user.IsBot && userMessage.Id == _lfgMessageId)
                await user.AddRoleAsync(lfgEmojis[(Emoji)reaction.Emote]);
        }

        public async Task UserRemovedReactionToLFG(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            if (!user.IsBot && userMessage.Id == _lfgMessageId)
                await user.RemoveRoleAsync(lfgEmojis[(Emoji)reaction.Emote]);
        }

        #endregion




    }
}
