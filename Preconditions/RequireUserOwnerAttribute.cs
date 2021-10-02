using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Preconditions
{
    // This Attribute is used to only allow commands if the
    // user is the guild owner.
    public class RequireUserOwnerAttribute : PreconditionAttribute
    {     
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command,
            IServiceProvider services)
        {
            if (context.User is SocketGuildUser gUser && context.Guild is SocketGuild sGuild)
            {
                if (sGuild.OwnerId == gUser.Id)
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    // User isn't owner
                    return Task.FromResult(PreconditionResult.FromError("No"));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Not usable in DMs"));
            }
        }
    }
}
