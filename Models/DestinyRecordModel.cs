using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    public class DestinyRecordModel
    {
        // Hash seems to be same as the id property in the row
        public string hash { get; set; }
        public string index { get; set; }
        public TitleInfo titleInfo { get; set; }

        /*
         *   This may be used eventually.
         *   Discord seems to be implementing icons for roles.
         *   
         * public DisplayProperties displayProperties { get; set; }
         */
    }
    public class TitleInfo
    {
        public bool hasTitle { get; set; }
        public TitlesByGender titlesByGender { get; set; }
    }

    // So far every title is the same regardless of gender.
    public class TitlesByGender
    {
        // public string Female { get; set; }
        public string Male { get; set; }
    }


    /*
     * This may be used eventually. 
     * Discord seems to be implementing icons for roles. 
     * 
     * 
        public class IconSequence
        {
            public List<string> frames { get; set; }
        }


        
        public class DisplayProperties
        {
            public string description { get; set; }
            public bool hasIcon { get; set; }
            public string icon { get; set; }
            public List<IconSequence> iconSequences { get; set; }
            public string name { get; set; }
        }
        */
}
