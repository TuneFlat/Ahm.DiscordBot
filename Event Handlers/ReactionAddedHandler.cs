using System;
using System.Threading.Tasks;
using Ahm.DiscordBot.Models;
using Ahm.DiscordBot.Services;
using Discord;
using Discord.WebSocket;

namespace Ahm.DiscordBot.Event_Handlers
{
    public class ReactionAddedHandler
    {
        private DiscordSocketClient _client;
        private SavedIdsModel _savedIdsModel;
        private IFileIOService _fileIOService;
       
        public ReactionAddedHandler(DiscordSocketClient client, IFileIOService fileIoService)
        {
            _client = client;
            _fileIOService = fileIoService;

            GetSavedIds();

            _client.ReactionAdded += ReactionAdded;
            
        }

        private async Task ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel,
            SocketReaction reaction)
        {          
            SocketGuildUser user = (SocketGuildUser)reaction.User;

            // Ensuring only users that add emojis on the correct message will get roles.
            if (!user.IsBot && _savedIdsModel.ListOfMessageIds.Contains(message.Id))
                await user.AddRoleAsync(_savedIdsModel.AllEmojis[(Emoji)reaction.Emote]);
        }

        private void GetSavedIds()
        {
            var savedIdsPath = $"{Environment.CurrentDirectory}\\SavedIds.json"; 
            _savedIdsModel = _fileIOService.ReadFile<SavedIdsModel>(savedIdsPath);
        }
    }
}
