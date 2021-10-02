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
    public class ReactionRoleModule : ModuleBase<SocketCommandContext>
    {
        #region LFG 

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
            if (reaction.UserId != 759220605802643496) // Bots ID
            {
                SocketGuildUser user = (SocketGuildUser)reaction.User;
                await user.AddRoleAsync(lfgEmojis[(Emoji)reaction.Emote]);
            }
        }

        public async Task UserRemovedReactionToLFG(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != 759220605802643496) // Bots ID
            {
                SocketGuildUser user = (SocketGuildUser)reaction.User;
                await user.RemoveRoleAsync(lfgEmojis[(Emoji)reaction.Emote]);
            }
        }

        #endregion


        #region Pronouns

        readonly Dictionary<Emoji, ulong> pronounEmojis = new Dictionary<Emoji, ulong>(){
            { new Emoji("1️⃣"),884295099318669313 },
            { new Emoji("2️⃣"), 884295182068101170},
            { new Emoji("3️⃣"), 884295223432314950}
        };

        [Command("ReactPronounRoles")]
        public async Task ReactPronounRoles()
        {
            await Context.Message.DeleteAsync();

            var pronounMessage = (RestUserMessage)await ReplyAsync(PronounMessageBuilder());
            await pronounMessage.AddReactionsAsync(pronounEmojis.Select(x => x.Key).ToArray());

            Context.Client.ReactionAdded += UserReacted;
            Context.Client.ReactionRemoved += UserRemovedReaction;
        }

        private string PronounMessageBuilder()
        {
            string message = "Pronoun Roles\n";
            string male = new Emoji("1️⃣") + " He/Him\n";
            string female = new Emoji("2️⃣") + " She/Her\n";
            string nonb = new Emoji("3️⃣") + " They/Them\n";
            // TODO: Add pronouns here when it's requested
            return message + male + female + nonb;
        }

        public async Task UserReacted(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != 759220605802643496) // Bots ID
            {

                SocketGuildUser user = (SocketGuildUser)reaction.User;
                await user.AddRoleAsync(pronounEmojis[(Emoji)reaction.Emote]);
            }

        }

        public async Task UserRemovedReaction(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            if (reaction.UserId != 759220605802643496) // Bots ID
            {
                SocketGuildUser user = (SocketGuildUser)reaction.User;
                await user.RemoveRoleAsync(pronounEmojis[(Emoji)reaction.Emote]);
            }
        }

        #endregion


    }
}
