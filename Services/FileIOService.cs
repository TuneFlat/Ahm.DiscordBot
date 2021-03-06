using Ahm.DiscordBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Services
{
    public class FileIOService : IFileIOService
    {
        public T ReadFile<T>(string path)
        {
            var jsonText = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<T>(jsonText);
        }

        public void WriteFile<T>(string path, T content, Formatting formattingOption = Formatting.Indented)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
            jsonSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            File.WriteAllText(path, JsonConvert.SerializeObject(content, formattingOption, jsonSettings));
        }
    }
}
