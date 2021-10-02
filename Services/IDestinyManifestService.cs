using Ahm.DiscordBot.Models;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Ahm.DiscordBot.Services
{
    public interface IDestinyManifestService
    {
        JToken DoRequest(string path);

        void UpdateManifest(string url);

        void CheckManifest();

        IList<BaseManifestDefinitionModel> QueryManifest(string query);

        IList<BaseManifestDefinitionModel> GetDefinitions(string tableName);

        // TODO: combine GetDefinition and GetSingleDefinition
        BaseManifestDefinitionModel GetDefinition(string tableName, string id);

        void TestManifestConnection();
    }
}
