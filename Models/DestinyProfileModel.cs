using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    public class DestinyProfileModel
    {
        public Response Response { get; set; }
    }

    public class Objective
    {
        public string objectiveHash { get; set; }
        public int progress { get; set; }
        public int completionValue { get; set; }
        public bool complete { get; set; }
        public bool visible { get; set; }
    }

    

    public class RecordProperties
    {
        public int state { get; set; }
        public List<Objective> objectives { get; set; }
        public int intervalsRedeemedCount { get; set; }
    }

    public class Data
    {
        public int score { get; set; }
        public int activeScore { get; set; }
        public int legacyScore { get; set; }
        public int lifetimeScore { get; set; }
        public Dictionary<string, RecordProperties> records { get; set; }
    }

    public class ProfileRecords
    {
        public Data data { get; set; }
    }

    public class Response
    {
        public ProfileRecords profileRecords { get; set; }
    }
}
