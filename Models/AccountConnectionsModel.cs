using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Models
{
    public class AccountConnectionsModel
    {
        public int id { get; set; }
        public string discord_account_id { get; set; }
        public string bungie_account_id { get; set; }
        public string destiny_membership_id { get; set; }
        public int destiny_membership_type { get; set; }
    }
}
