using Ahm.DiscordBot.Models;
using Microsoft.Extensions.Logging;
using System.Data.SQLite;
using System;

namespace Ahm.DiscordBot.Services
{
    public class AccountConnectionService : IAccountConnectionService
    {
        private ILogger _logger;

        public AccountConnectionService(ILogger<AccountConnectionService> logger)
        {
            _logger = logger;
        }


        private SQLiteConnection CreateConnection()
        {
            SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=Accounts.sqlite3");

            try
            {
                sqlite_conn.Open();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            return sqlite_conn;
        }




        #region Get

        public string GetBungieId(ulong discordId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = String.Format("SELECT bungie_account_id FROM account_connections WHERE discord_account_id == {0}", discordId);

            var data = command.ExecuteReader();
            string result = String.Empty;
            while (data.Read())
            {
                result = data.GetString(0);
            }

            connection.Close();
            return result;
        }

        public string GetDestinyMembershipId(ulong discordId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = String.Format("SELECT destiny_membership_id FROM account_connections WHERE discord_account_id == {0}", discordId.ToString());


            var data = command.ExecuteReader();
            string result = String.Empty;
            while (data.Read())
            {
                result = data.GetString(0);
            }

            connection.Close();
            return result;
        }

        public AccountConnectionsModel GetDestinyMembershipWithType(ulong discordId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = $"SELECT destiny_membership_id, destiny_membership_type FROM account_connections WHERE discord_account_id == {discordId}";


            AccountConnectionsModel accountConnectionsModel = null;
            var data = command.ExecuteReader();
            while (data.Read())
            {
                accountConnectionsModel = new AccountConnectionsModel()
                {
                    destiny_membership_id = data.GetString(0),
                    destiny_membership_type = data.GetInt32(1)
                };
            }

            connection.Close();
            return accountConnectionsModel;
        }


        #endregion


        #region Delete

        public void DeleteAccountConnections(ulong discordId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            command.CommandText = String.Format("DELETE FROM account_connections " +
                "WHERE discord_account_id == {0}", discordId);

            command.ExecuteReader();
            connection.Close();
        }

        #endregion

        public void TestConnection()
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT name FROM sqlite_master WHERE type ='table'";

            var data = command.ExecuteReader();
            string result = String.Empty;
            while (data.Read())
            {
                result = data.GetString(0);
                Console.WriteLine(string.Format("Tables:{0}", result));
            }

            connection.Close();
        }

    }
}
