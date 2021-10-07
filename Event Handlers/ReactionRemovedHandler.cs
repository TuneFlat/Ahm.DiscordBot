using System;
using System.Threading.Tasks;
using Ahm.DiscordBot.Models;
using Ahm.DiscordBot.Services;
using Discord;
using Discord.WebSocket;

namespace Ahm.DiscordBot.Event_Handlers
{
    public class ReactionRemovedHandler
    {
        private DiscordSocketClient _client;
        private SavedIdsModel _savedIdsModel;
        private IFileIOService _fileIOService;

        public ReactionRemovedHandler(DiscordSocketClient client, IFileIOService fileIoService)
        {
            _client = client;
            _fileIOService = fileIoService;

            GetSavedIds();
            _client.ReactionRemoved += ReactionRemoved;

        }

        private async Task ReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            if (!user.IsBot && _savedIdsModel.ListOfMessageIds.Contains(message.Id))
                await user.RemoveRoleAsync(_savedIdsModel.AllEmojis[(Emoji)reaction.Emote]);
        }
        private void GetSavedIds()
        {
            var savedIdsPath = $"{Environment.CurrentDirectory}\\SavedIds.json";
            _savedIdsModel = _fileIOService.ReadFile<SavedIdsModel>(savedIdsPath);
        }

    }
}
