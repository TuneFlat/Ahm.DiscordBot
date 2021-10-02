using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    /*
     * This model is used for all Manifest queries.
     * Specific Definition models will be created when the need arises.
     */
    public class BaseManifestDefinitionModel
    {
        // Some fields in the database are Int, some are Text.
        public string Id { get; set; }

        public string Json { get; set; }

        public BaseManifestDefinitionModel(string id, string json)
        {
            Id = id;
            Json = json;
        }
    }
}
