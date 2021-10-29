using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ahm.DiscordBot.Preconditions;
using Discord;
using Discord.Commands;

namespace Ahm.DiscordBot.Modules.UtilityModules
{
    
    [RequireUserOwner]
    public class ClearModule : ModuleBase<SocketCommandContext>
    {
        [Command("clear")]
        public async Task Clear(int messageAmount = 100)
        {
            // 100 is the max number of messages able to be deleted in one request.
            // This cap is to avoid being rate limited.
            if (messageAmount > 1000)
                throw new InvalidOperationException("Too many messages requested.");

            var messagesToDelete = new List<IMessage>();
            var messages = await Context.Channel.GetMessagesAsync(messageAmount).FlattenAsync();
            foreach(var message in messages)
            {
                // Discord only allows bots to delete messages created within
                // the last 14 days.
                if ((DateTime.Now - message.Timestamp).Days < 14)
                    messagesToDelete.Add(message);
            }

            var channel = Context.Channel as ITextChannel;
            await channel.DeleteMessagesAsync(messagesToDelete);
        }
    }
}
