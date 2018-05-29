using System;
using System.Threading.Tasks;
using Discord.Commands;
using PassiveBOT.Discord.Extensions.EmojiTools;

namespace PassiveBOT.Discord.TypeReaders
{
    public class EmojiTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services)
        {
            try
            {
                var Result = EmojiExtensions.FromText(input);
                return Task.FromResult(TypeReaderResult.FromSuccess(Result));
            }
            catch (Exception e)
            {
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a Emoji."));
            }
        }
    }
}