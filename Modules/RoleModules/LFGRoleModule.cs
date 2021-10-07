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
    public class LFGRoleModule : ModuleBase<SocketCommandContext>
    {
        private ulong _lfgMessageId;

        private SavedIdsModel _savedIdsModel;
        private IFileIOService _fileIOService;


        [Command("add lfg reactions")]
        public async Task AddLFGReactions([Remainder] ulong messageId)
        {
            _lfgMessageId = messageId;

            _fileIOService = new FileIOService();
            GetSavedIds();

            var messageForRoles = (IUserMessage)await Context.Channel.GetMessageAsync(messageId);
            await messageForRoles.AddReactionsAsync(_savedIdsModel.lfgRoleProperties.LFGEmojis.Select(x => x.Key).ToArray());
        }
        private void GetSavedIds()
        {
            var savedIdsPath = $"{Environment.CurrentDirectory}\\SavedIds.json";
            _savedIdsModel = _fileIOService.ReadFile<SavedIdsModel>(savedIdsPath);
        }

        [Command("update lfg reactions")]
        public async Task UpdateLFGReactions(string emojiEmoteString, [Remainder] ulong messageId)
        {
            List<Emote> emoteList = new List<Emote>();
            List<Emoji> emojiList = new List<Emoji>();

            // This pattern captures all current emojis
            var emojiPattern = @"(\u00a9|\u00ae|[\u2000-\u3300]|\ud83c[\ud000-\udfff]|\ud83d[\ud000-\udfff]|\ud83e[\ud000-\udfff])";
            var emojiRegex = new Regex(emojiPattern);
            foreach (Match match in emojiRegex.Matches(emojiEmoteString))
            {
                if (match.Value == string.Empty) continue;
                emojiList.Add(new Emoji(match.Value));
                emojiEmoteString = emojiEmoteString.Replace(match.Value, "");
            }
            await Context.Message.AddReactionsAsync(emojiList.ToArray());

            // This patter is for custom Emojis, labeled as Emotes. 
            // The string looks like: <Name:IdUlong>
            var emotePattern = @"[(?=<)](.*?)[(?<=>)]";
            var emoteRegex = new Regex(emotePattern);
            foreach (Match match in emoteRegex.Matches(emojiEmoteString))
            {
                if (match.Value == string.Empty) continue;
                if (Emote.TryParse(match.Value, out var emoteValue))
                {
                    emoteList.Add(emoteValue);
                }
            }

            await Context.Message.AddReactionsAsync(emoteList.ToArray());
        }

    }
}
// TODO: write to SavedIdsJson when changing lfg message/emojis

