using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace Ahm.DiscordBot.Modules.DestinyModules
{
    public class LinkBungieDiscordModule : ModuleBase<SocketCommandContext>
    {
        [Command("link")]
        public async Task Link()
        {
            var embed = new EmbedBuilder();
            embed.WithTitle("Click Here To Register");
            embed.WithUrl("https://discord.com/api/oauth2/authorize?client_id=759220605802643496&redirect_uri=https%3A%2F%2Ftuneflat.xyz%2FDiscordRedirect%2FGetDiscordAuth&response_type=code&scope=identify");
            await Context.Channel.SendMessageAsync("", embed: embed.Build());
        }
    }
}
