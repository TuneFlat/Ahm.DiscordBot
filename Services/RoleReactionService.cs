using Ahm.DiscordBot.Models;
using Discord;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ahm.DiscordBot.Services
{
    public class RoleReactionService : IRoleReactionService
    {
        private ILogger _logger;

        public RoleReactionService(ILogger<RoleReactionService> logger)
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

        public List<RoleReactionModel> GetRoleReactions()
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM role_reactions";
            var data = command.ExecuteReader();

            List<RoleReactionModel> roleReactionModels = new();

            while (data.Read())
            {
                var text = SafeGetString(data, 4);
                Emote.TryParse(text, out var dbEmote);

                var emojiText = SafeGetString(data, 3);
                var emoji = emojiText == string.Empty ? null : new Emoji(emojiText);

                roleReactionModels.Add(new RoleReactionModel()
                {
                    ReactMessageId = Convert.ToUInt64(data.GetValue(1)),
                    DiscordRoleId = Convert.ToUInt64(data.GetValue(2)),
                    Emoji = emoji,
                    Emote = dbEmote ?? null
                });
            }

            connection.Close();

            return roleReactionModels;
        }

        public List<RoleReactionModel> GetRoleReactions(ulong reactMessageId = ulong.MinValue, ulong discordRoleId = ulong.MinValue)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            if (reactMessageId != ulong.MinValue && discordRoleId != ulong.MinValue)
                command.CommandText = $"SELECT * FROM role_reactions WHERE react_message_id == {reactMessageId} " +
                    $"AND discord_role_id == {discordRoleId}";
            else if (reactMessageId != ulong.MinValue)
                command.CommandText = $"SELECT * FROM role_reactions WHERE react_message_id == {reactMessageId}";
            else if (discordRoleId != ulong.MinValue)
                command.CommandText = $"SELECT * FROM role_reactions WHERE discord_role_id == {discordRoleId}";
            else
                throw new InvalidOperationException("reactMessageId or discordRoleId must have a value");

            var data = command.ExecuteReader();

            List<RoleReactionModel> roleReactionModels = new();

            while (data.Read())
            {
                var text = SafeGetString(data, 4);
                Emote.TryParse(text, out var dbEmote);

                var emojiText = SafeGetString(data, 3);
                var emoji = emojiText == string.Empty ? null : new Emoji(emojiText);

                roleReactionModels.Add(new RoleReactionModel()
                {
                    ReactMessageId = Convert.ToUInt64(data.GetValue(1)),
                    DiscordRoleId = Convert.ToUInt64(data.GetValue(2)),
                    Emoji = emoji,
                    Emote = dbEmote ?? null
                });
            }

            connection.Close();

            return roleReactionModels;
        }

        public RoleReactionModel TestConnection()
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = "SELECT * FROM role_reactions";
            var data = command.ExecuteReader();

            RoleReactionModel roleReactionModels = new();

            while (data.Read())
            {
                var text = SafeGetString(data, 4);
                Emote.TryParse(text, out var dbEmote);

                roleReactionModels = new RoleReactionModel()
                {
                    ReactMessageId = Convert.ToUInt64(data.GetValue(1)),
                    DiscordRoleId = Convert.ToUInt64(data.GetValue(2)),
                    Emoji = new Emoji(data.GetString(3)),
                    Emote = dbEmote ?? null
                };
            }

            connection.Close();

            return roleReactionModels;
        }

        public string SafeGetString(SQLiteDataReader reader, int colIndex)
        {
            if (!reader.IsDBNull(colIndex))
                return reader.GetString(colIndex);
            return string.Empty;
        }

        public void AddRoleReaction(int reactMessageId, int discordRoleId, Emoji emoji = null)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"INSERT into role_reactions (react_message_id, discord_role_id, emoji) " +
                    $"VALUES({reactMessageId}, {discordRoleId}, \"{emoji}\")";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();
        }

        public void AddRoleReaction(ulong reactMessageId, ulong discordRoleId, Emote emote)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"INSERT INTO role_reactions (react_message_id, discord_role_id, emote) " +
                                    $"VALUES({reactMessageId}, {discordRoleId}, \"{emote}\")";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();
        }

        public void AddRoleReaction(ulong reactMessageId, ulong discordRoleId, Emoji emoji)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"INSERT into role_reactions (react_message_id, discord_role_id, emoji) " +
                                    $"VALUES({reactMessageId}, {discordRoleId}, \"{emoji}\")";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();
        }

        public void AddRoleReaction(RoleReactionModel roleReactionModel)
        {
            if (roleReactionModel.Emoji != null)
                AddRoleReaction(roleReactionModel.ReactMessageId, roleReactionModel.DiscordRoleId, roleReactionModel.Emoji);
            if (roleReactionModel.Emote != null)
                AddRoleReaction(roleReactionModel.ReactMessageId, roleReactionModel.DiscordRoleId, roleReactionModel.Emote);
        }

        public void AddRoleReactions(List<RoleReactionModel> roleReactionModels)
        {
            foreach (var roleReactionModel in roleReactionModels)
            {
                if (roleReactionModel.Emoji != null)
                    AddRoleReaction(roleReactionModel.ReactMessageId, roleReactionModel.DiscordRoleId, roleReactionModel.Emoji);
                if (roleReactionModel.Emote != null)
                    AddRoleReaction(roleReactionModel.ReactMessageId, roleReactionModel.DiscordRoleId, roleReactionModel.Emote);
            }
        }

        public void UpdateRoleReactionMessageIds(ulong newMessageId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"UPDATE role_reactions " +
                    $"SET react_message_id = {newMessageId}";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();
        }

        public void UpdateRoleReactionMessageIds(ulong oldMessageId, ulong newMessageId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"UPDATE role_reactions " +
                    $"SET react_message_id = {newMessageId} " +
                    $"WHERE react_message_id == {oldMessageId}";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();
        }

        public List<RoleReactionModel> DeleteMessageRoleReactions(ulong reactMessageId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"DELETE FROM role_reactions WHERE (react_message_id LIKE {reactMessageId})";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();

            return GetRoleReactions();
        }

        public List<RoleReactionModel> DeleteDiscordRoleReactions(ulong discordRoleId)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"DELETE FROM role_reactions WHERE (discord_role_id LIKE {discordRoleId})";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();

            return GetRoleReactions();
        }

        public List<RoleReactionModel> DeleteRoleReaction(Emoji emoji = null)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();
            var emojiName = emoji.Name;
            try
            {
                command.CommandText = $"DELETE FROM role_reactions WHERE emoji == \"{emojiName}\"";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();

            return GetRoleReactions();
        }

        public List<RoleReactionModel> DeleteRoleReaction(Emote emote = null)
        {
            var connection = CreateConnection();
            SQLiteCommand command = connection.CreateCommand();

            try
            {
                command.CommandText = $"DELETE FROM role_reactions WHERE emote == \"{emote}\"";
                command.ExecuteNonQuery();
            }
            catch (Exception exception)
            {
                _logger.LogInformation(exception.ToString());
            }

            connection.Close();

            return GetRoleReactions();
        }


        public List<RoleReactionModel> DeleteRoleReactions<T>(List<T> reactions)
        {
            foreach (var reaction in reactions)
            {
                if (reaction.GetType() == typeof(Emoji) && reaction != null)
                {
                    DeleteRoleReaction(reaction as Emoji);
                }
                if (reaction.GetType() == typeof(Emote) || reaction.GetType() == typeof(IEmote)
                    && reaction != null)
                {
                    //TODO: Test if IEmote works
                    DeleteRoleReaction(reaction as Emote);
                }
            }

            return GetRoleReactions();
        }


    }
}
