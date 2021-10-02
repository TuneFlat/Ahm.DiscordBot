using System.Threading.Tasks;
using Ahm.DiscordBot.Preconditions;
using Discord;
using Discord.Commands;

namespace Ahm.DiscordBot.Modules.UtilityModules
{
    
    [RequireUserOwner]
    public class ClearModule : ModuleBase<SocketCommandContext>
    {
        //TODO: rename to clear no jutsu

        [Command("clear")]
        public async Task Clear(int messageAmount = 100)
        {
            var channel = Context.Channel as ITextChannel;
            var messages = await Context.Channel.GetMessagesAsync(messageAmount).FlattenAsync();

            await channel.DeleteMessagesAsync(messages);
        }
    }
}
