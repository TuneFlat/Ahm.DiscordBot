using Ahm.DiscordBot.Models;
using RestSharp;
using System.Collections.Generic;

namespace Ahm.DiscordBot.Services
{
    public interface IDestinyService
    {
        bool HasTitle(string titleName, ulong discordId);
        IRestResponse ExecuteRequest(string requestEndpoint);
        AccountConnectionsModel GetDestinyMembershipWithType(ulong discordId);
        Dictionary<string, List<string>> GetTitleHashsFromManifest();
        List<string> GetUserTitles(ulong discordId);
    }
}
