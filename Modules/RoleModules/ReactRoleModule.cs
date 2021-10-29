using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Ahm.DiscordBot.Models;
using Ahm.DiscordBot.Preconditions;
using Ahm.DiscordBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Ahm.DiscordBot.Modules.RoleModules
{
    [RequireUserOwner]
    public class ReactRoleModule : ModuleBase<SocketCommandContext>
    {
        private List<RoleReactionModel> _roleReactionModels;
        private IRoleReactionService _roleReactionService;

        public ReactRoleModule(IRoleReactionService roleReactionService)
        {
            _roleReactionService = roleReactionService;
        }

        [Command("add react reactions")]
        [Alias("Add React Reactions")]
        [Summary("Adds reactions for the specified messageId. " +
            "If a Message Id isn't given all reactions will be added to their respective messages.")]
        public async Task AddReactReactions(ulong messageId = 0)
        {
            if (_roleReactionModels == null)
                _roleReactionModels = _roleReactionService.GetRoleReactions();

            
            var reactMessageIds = _roleReactionModels.Select(x => x.ReactMessageId).Distinct();

            // replacing reactions on each reactMessageId.
            foreach(var reactMessageId in reactMessageIds) { 
                IUserMessage messageForRoles;
                if (messageId == 0)
                    messageForRoles = (IUserMessage)await Context.Channel.GetMessageAsync(reactMessageId);
                else
                    messageForRoles = (IUserMessage)await Context.Channel.GetMessageAsync(messageId);

                await messageForRoles.RemoveAllReactionsAsync();

                await messageForRoles.AddReactionsAsync(_roleReactionModels.Where(x => x.ReactMessageId == reactMessageId && x.Emoji != null)
                    .Select(x => x.Emoji).ToArray());
                await messageForRoles.AddReactionsAsync(_roleReactionModels.Where(x => x.ReactMessageId == reactMessageId && x.Emote != null)
                    .Select(x => x.Emote).ToArray());

                // This emoji is used in ReactionAdded/RemovedEventHandler to update its _roleReactionModels
                var updateEmoji = new Emoji("📝");
                await messageForRoles.AddReactionAsync(updateEmoji);
                _ = Task.Delay(TimeSpan.FromSeconds(2)).ContinueWith(task => messageForRoles.RemoveReactionAsync(updateEmoji, Context.Guild.CurrentUser));
            }
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => Context.Message.DeleteAsync());
        }

        [Command("add react roles")]
        [Alias("Add React Roles")]
        [Summary("Adds react roles. Example format: 'prefix' add react roles 12345 ⭐|StarRole. " +
            "If the role name has a space use _. Example: ⭐|Star_Role.")]
        public async Task AddReactRoles(ulong messageReactId, params string[] emojiEmoteRoleId)
        {
            if (_roleReactionModels == null)
                _roleReactionModels = _roleReactionService.GetRoleReactions();

            var newRoleReactions = new List<RoleReactionModel>();

            foreach (var value in emojiEmoteRoleId)
            {
                // reaction|rolename
                var split = value.Split("|");
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == split[1].ToLower().Replace("_", " "));
                if (split == null || role == null)
                {
                    await Context.Channel.SendMessageAsync($"{role.Name} not found");
                }
                else if (Emote.TryParse(split[0], out var emote))
                {
                    newRoleReactions.Add(new RoleReactionModel()
                    {
                        ReactMessageId = messageReactId,
                        DiscordRoleId = role.Id,
                        Emote = emote,
                    });
                }

                else
                {
                    try
                    {
                        newRoleReactions.Add(new RoleReactionModel()
                        {
                            ReactMessageId = messageReactId,
                            DiscordRoleId = role.Id,
                            Emoji = new Emoji(split[0]),
                        });
                    }
                    catch (Exception exception)
                    {
                        await Context.Channel.SendMessageAsync(exception.ToString());
                    }
                }
            }
            _roleReactionService.AddRoleReactions(newRoleReactions);
            _roleReactionModels.AddRange(newRoleReactions);
            await AddReactReactions(messageReactId);
            await Context.Message.AddReactionAsync(new Emoji("✅"));
            _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(task => Context.Message.DeleteAsync());
        }

        [Command("replace message reactions")]
        [Alias("Replace Message Reactions")]
        [Summary("Replaces each react role for the given Message Id. Example format: 'prefix' replace message reactions 12345 ⭐|StarRole. " +
            "If the role name has a space use _. Example: ⭐|Star_Role.")]
        public async Task ReplaceReactReactions(ulong messageId, params string[] emojiEmoteRoleId)
        {
            var oldMessage = await Context.Channel.GetMessageAsync(messageId);
            if (oldMessage == null)
                throw new InvalidOperationException($"Message with the id {messageId} not found. " +
                    "The message id provided is invalid");

            await oldMessage.RemoveAllReactionsAsync();

            _roleReactionService.DeleteMessageRoleReactions(messageId);
            await AddReactRoles(messageId, emojiEmoteRoleId);
        }

        [Command("set all message ids")]
        [Alias("Set All Message Ids", "Set All Message IDs ")]
        [Summary("Sets every React Role to use the given Message Id.")]
        public async Task SetAllReactMessageId(ulong newMessageId)
        {
            var message = await Context.Channel.GetMessageAsync(newMessageId);
            if (message == null)
                throw new InvalidOperationException($"Message with the id {message} not found. " +
                    "The message id provided is invalid");

            _roleReactionModels = _roleReactionService.GetRoleReactions();
            var uniqueMessageIds = _roleReactionModels.Select(x => x.ReactMessageId).Distinct();
            foreach (var messageId in uniqueMessageIds)
            {
                var oldMessage = await Context.Channel.GetMessageAsync(messageId);
                await oldMessage.RemoveAllReactionsAsync();
            }

            _roleReactionService.UpdateRoleReactionMessageIds(newMessageId);
            await AddReactReactions(newMessageId);
        }

        [Command("update message id")]
        [Alias("update message ID")]
        [Summary("Sets every React Role with the oldMessageId to newMessageId.")]
        public async Task UpdateReactMessageId(ulong oldMessageId, ulong newMessageId)
        {
            var oldMessage = await Context.Channel.GetMessageAsync(oldMessageId);
            if (oldMessage == null)
                throw new InvalidOperationException($"Message with the id {oldMessage} not found. " +
                    "The message id provided is invalid");

            var newMessage = await Context.Channel.GetMessageAsync(newMessageId);
            if (newMessage == null)
                throw new InvalidOperationException($"Message with the id {newMessageId} not found. " +
                    "The message id provided is invalid");

            await oldMessage.RemoveAllReactionsAsync();

            _roleReactionService.UpdateRoleReactionMessageIds(oldMessageId, newMessageId);
            await AddReactReactions(newMessageId);
        }

        [Command("list react roles")]
        [Alias("List React Roles")]
        [Summary("Lists every React Role.")]
        public async Task ListReactRoles()
        {
            // TODO: Add column to description that's an anchor tag to the messageid
            // TODO: add param to list react roles for that message id
            _roleReactionModels = _roleReactionService.GetRoleReactions();
            if (_roleReactionModels == null)
                throw new InvalidOperationException($"Role Reactions is empty.");

            var reactRoles = Context.Guild.Roles.Where(x => _roleReactionModels.Select(x => x.DiscordRoleId).Contains(x.Id)).ToList();

            var embed = new EmbedBuilder();

            string description = string.Empty;
            foreach (var roleReactionModel in _roleReactionModels)
            {
                if (roleReactionModel.Emoji != null)
                    description += $"{roleReactionModel.Emoji} | {reactRoles.First(x => x.Id == roleReactionModel.DiscordRoleId)}\n";
                if (roleReactionModel.Emote != null)
                    description += $"{roleReactionModel.Emote} | {reactRoles.First(x => x.Id == roleReactionModel.DiscordRoleId)}\n";
            }

            embed.WithDescription(description);

            await ReplyAsync(embed: embed.Build());
        }

        [Command("delete react reaction")]
        [Alias("Delete React Reaction")]
        [Summary("Deletes every React Role that uses the emote.")]
        public async Task DeleteReactReaction(Emote emote)
        {
            _roleReactionModels = _roleReactionService.DeleteRoleReaction(emote);

            await AddReactReactions();
        }

        [Command("delete react reaction")]
        [Alias("Delete React Reaction")]
        [Summary("Deletes every React Role that uses the emoji.")]
        public async Task DeleteReactReaction(Emoji emoji)
        {
            _roleReactionModels = _roleReactionService.DeleteRoleReaction(emoji);

            await AddReactReactions();
        }

        [Command("delete react role")]
        [Alias("Delete React Role")]
        [Summary("Deletes every React Role that uses the roleName.")]
        public async Task DeleteReactRole(string roleName)
        {
            _roleReactionModels = _roleReactionService.GetRoleReactions();
            var reactRoles = Context.Guild.Roles.Where(x => _roleReactionModels.Select(x => x.DiscordRoleId).Contains(x.Id));
            if (reactRoles == null)
                throw new InvalidOperationException($"React roles does not have any roles found in the server {Context.Guild.Name}");

            var roleId = reactRoles?.FirstOrDefault(x => x.Name.ToLower() == roleName.ToLower()).Id;
            if (roleId == null)
                throw new InvalidOperationException($"Role: {roleName} not found.");

            _roleReactionService.DeleteDiscordRoleReactions(roleId.GetValueOrDefault());
            await AddReactReactions();
        }
    }
}


