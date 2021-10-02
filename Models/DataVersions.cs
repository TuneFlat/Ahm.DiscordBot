using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    public class DataVersions
    {
        public DestinyManifestVersion DestinyManifestVersion { get; set; }
    }

    public class DestinyManifestVersion
    {
        public string Version { get; set; }


        private DateTime _lastModified;
        public DateTime LastModified
        {
            get => _lastModified; 
            set => _lastModified = DateTime.Parse(value.ToString("MM/dd/yyyy HH:mm:ss"));
        }

        private DateTime _lastChecked;
        public DateTime LastChecked
        {
            get => _lastChecked; 
            set => _lastChecked = DateTime.Parse(value.ToString("MM/dd/yyyy HH:mm:ss"));
        }
    }
}
