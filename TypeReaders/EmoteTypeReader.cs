using Discord.Commands;
using System;
using Discord;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.TypeReaders
{
    public class EmoteTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            Emote result;
            if (Emote.TryParse(input, out result))
                return Task.FromResult(TypeReaderResult.FromSuccess(result));

            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a Emote."));
        }
    }
}
