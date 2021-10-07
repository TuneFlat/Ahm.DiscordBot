using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Services
{
    public interface IFileIOService
    {
        T ReadFile<T>(string path);

        void WriteFile<T>(string path, T content, Formatting formattingOption = Formatting.Indented);
    }
}
