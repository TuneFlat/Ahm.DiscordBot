using Ahm.DiscordBot.Models;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Services
{
    public interface IRoleReactionService
    {
        List<RoleReactionModel> GetRoleReactions();
        List<RoleReactionModel> GetRoleReactions(ulong reactMessageId = ulong.MinValue, ulong discordRoleId = ulong.MinValue);


        void AddRoleReaction(ulong reactMessageId, ulong discordRoleId, Emoji emoji);
        void AddRoleReaction(ulong reactMessageId, ulong discordRoleId, Emote emote);
        void AddRoleReaction(RoleReactionModel roleReactionModel);
        void AddRoleReactions(List<RoleReactionModel> roleReactionModels);

        void UpdateRoleReactionMessageIds(ulong newMessageId);
        void UpdateRoleReactionMessageIds(ulong oldMessageId, ulong newMessageId);

        List<RoleReactionModel> DeleteMessageRoleReactions(ulong reactMessageId);
        List<RoleReactionModel> DeleteDiscordRoleReactions(ulong discordRoleId);
        List<RoleReactionModel> DeleteRoleReaction(Emoji emoji);
        List<RoleReactionModel> DeleteRoleReaction(Emote emote);
        List<RoleReactionModel> DeleteRoleReactions<T>(List<T> reactions);
    }
}
