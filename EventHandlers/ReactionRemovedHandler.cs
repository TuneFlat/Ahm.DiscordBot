using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ahm.DiscordBot.Models;
using Ahm.DiscordBot.Services;
using Discord;
using Discord.WebSocket;

namespace Ahm.DiscordBot.EventHandlers
{
    public class ReactionRemovedHandler
    {
        private DiscordSocketClient _client;
        private List<RoleReactionModel> _roleReactionModels;
        private IRoleReactionService _roleReactionService;

        public ReactionRemovedHandler(DiscordSocketClient client, IRoleReactionService roleReactionService)
        {
            _client = client;
            _roleReactionService = roleReactionService;
            _roleReactionModels = GetRoleReactionModels();
            _client.ReactionRemoved += ReactionRemoved;
        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            var messageIds = _roleReactionModels.Select(x => x.ReactMessageId);

            // Ensuring only reactions removed on the messages in savedIdsModel are processed
            if (messageIds.Contains(message.Id))
            {

                if (_roleReactionModels.Select(x => x.Emoji).Contains(reaction.Emote))
                {
                    if (user.IsBot) return;
                    var roleId = _roleReactionModels.FirstOrDefault(x => x.Emoji.Equals(reaction.Emote)).DiscordRoleId;
                    await user.RemoveRoleAsync(roleId);
                }
                else if (_roleReactionModels.Select(x => x.Emote).Contains(reaction.Emote))
                {
                    if (user.IsBot) return;
                    var roleId = _roleReactionModels.FirstOrDefault(x => x.Emote.Equals(reaction.Emote)).DiscordRoleId;
                    await user.RemoveRoleAsync(roleId);
                }
                else
                {
                    SocketGuild socketGuild = user.Guild;
                    var bot = await _client.GetApplicationInfoAsync();
                    if (socketGuild.OwnerId == user.Id || user.Id == bot.Id
                        && reaction.Emote.Name == "📝")// :pencil: or :memo:
                    {
                        _roleReactionModels = GetRoleReactionModels();
                    }
                }
            }
        }


        private List<RoleReactionModel> GetRoleReactionModels()
        {
            return _roleReactionService.GetRoleReactions();
        }
    }

}
