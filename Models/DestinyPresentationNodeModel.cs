using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    
    public class DestinyPresentationNodeModel
    {
        public Children children { get; set; }
        public string completionRecordHash { get; set; }
    }
    public class PresentationNode
    {
        public string presentationNodeHash { get; set; }
    }

    public class Children
    {
        public List<PresentationNode> presentationNodes { get; set; }
        public List<Records> records { get; set; }
    }

    public class Records
    {
        public string recordHash { get; set; }
    }



}
