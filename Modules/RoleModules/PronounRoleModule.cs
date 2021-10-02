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

            _pronounMessageId = pronounMessage.Id;

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
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            if (!user.IsBot && userMessage.Id == _pronounMessageId)
                await user.AddRoleAsync(pronounEmojis[(Emoji)reaction.Emote]);
        }

        public async Task UserRemovedReaction(Cacheable<IUserMessage, ulong> userMessage, ISocketMessageChannel channel, SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            if (!user.IsBot && userMessage.Id == _pronounMessageId)
                await user.RemoveRoleAsync(pronounEmojis[(Emoji)reaction.Emote]);

        }
    }
}
