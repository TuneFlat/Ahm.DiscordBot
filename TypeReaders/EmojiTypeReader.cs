using Discord.Commands;
using System;
using Discord;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.TypeReaders
{
    public class EmojiTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                Emoji emoji = new Emoji(input);
                return Task.FromResult(TypeReaderResult.FromSuccess(emoji));
            }
            catch(Exception exception)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a Emoji."));
            }
        }
    }
}
