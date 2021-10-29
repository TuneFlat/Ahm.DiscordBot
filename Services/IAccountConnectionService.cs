using Ahm.DiscordBot.Models;

namespace Ahm.DiscordBot.Services
{
    public interface IAccountConnectionService
    {
        #region Get

        string GetBungieId(ulong discordId);

        string GetDestinyMembershipId(ulong discordId);

        AccountConnectionsModel GetDestinyMembershipWithType(ulong discordId);

        #endregion


        #region Delete

        void DeleteAccountConnections(ulong discordId);

        #endregion
    }
}
