using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ahm.DiscordBot.Models;
using Ahm.DiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Ahm.DiscordBot.EventHandlers
{
    public class ReactionAddedHandler
    {
        private DiscordSocketClient _client;
        private List<RoleReactionModel> _roleReactionModels;
        private IRoleReactionService _roleReactionService;

        public ReactionAddedHandler(DiscordSocketClient client, IRoleReactionService roleReactionService)
        {
            _client = client;
            _roleReactionService = roleReactionService;
            _roleReactionModels = GetRoleReactionModels();
            _client.ReactionAdded += ReactionAdded;
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            var messageIds = _roleReactionModels.Select(x => x.ReactMessageId);

            // Ensuring only users that add emojis on the correct message will get roles.
            if (messageIds.Contains(message.Id))
            {
                if (_roleReactionModels.Select(x => x.Emoji).Contains(reaction.Emote))
                {
                    if (user.IsBot) return;
                    var roleId = _roleReactionModels.FirstOrDefault(x => x.Emoji.Equals(reaction.Emote)).DiscordRoleId;
                    await user.AddRoleAsync(roleId);
                }
                else if (_roleReactionModels.Select(x => x.Emote).Contains(reaction.Emote))
                {
                    if (user.IsBot) return;
                    var roleId = _roleReactionModels.FirstOrDefault(x => x.Emote.Equals(reaction.Emote)).DiscordRoleId;
                    await user.AddRoleAsync(roleId);
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
