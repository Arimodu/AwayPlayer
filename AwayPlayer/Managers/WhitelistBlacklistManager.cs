using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using Zenject;

namespace AwayPlayer.Managers
{
    public class WhitelistBlacklistManager : IInitializable
    {
        private readonly DatabaseManager DBMgr;
        private string BlacklistTableName;
        private string WhitelistTableName;
        private readonly SiraLog Log;
        public List<string> Blacklist { get; private set; }
        public List<string> Whitelist { get; private set; }

        public WhitelistBlacklistManager(SiraLog log, DatabaseManager dbmgr)
        {
            Log = log;
            DBMgr = dbmgr;
        }

        public void Initialize()
        {
            var userInfo = BS_Utils.Gameplay.GetUserInfo.GetPlatformUserModel().GetUserInfo();
            userInfo.Wait();
            var userId = userInfo.Result.platformUserId;
            BlacklistTableName = $"Blacklist_{userId}";
            WhitelistTableName = $"Whitelist_{userId}";

            // Create the table if it doesn't exist
            using var command = new SQLiteCommand($"CREATE TABLE IF NOT EXISTS {BlacklistTableName} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Key TEXT UNIQUE, Timestamp DATETIME)", DBMgr.Database);
            ExecuteSQLiteCommand(command, "Creating blacklist table");

            using var command2 = new SQLiteCommand($"CREATE TABLE IF NOT EXISTS {WhitelistTableName} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Key TEXT UNIQUE, Timestamp DATETIME)", DBMgr.Database);
            ExecuteSQLiteCommand(command2, "Creating whitelist table");
        }

        private void ExecuteSQLiteCommand(SQLiteCommand command, string actionDescription)
        {
            try
            {
                command.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    // Key already exists
                    Log.Warn($"{actionDescription} - Key already exists.");
                }
                else if (ex.Message.Contains("FOREIGN KEY constraint failed"))
                {
                    // Key does not exist
                    Log.Warn($"{actionDescription} - Key does not exist.");
                }
                else
                {
                    // Other SQLite errors
                    Log.Error($"{actionDescription} - SQLite error: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Other exceptions
                Log.Error($"{actionDescription} - An error occurred: {ex.Message}");
            }
        }

        public void AddToWhitelist(string songHash)
        {
            // Add key to the whitelist table
            using var command = new SQLiteCommand($"INSERT INTO {WhitelistTableName} (Key, Timestamp) VALUES (@Key, @Timestamp)", DBMgr.Database);
            command.Parameters.AddWithValue("@Key", songHash);
            command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
            ExecuteSQLiteCommand(command, $"Adding {songHash} to whitelist");

            if (Whitelist != null)
                Whitelist.Add(songHash);
        }

        public void RemoveFromWhitelist(string songHash)
        {
            // Remove key from the whitelist table
            using var command = new SQLiteCommand($"DELETE FROM {WhitelistTableName} WHERE Key = @Key", DBMgr.Database);
            command.Parameters.AddWithValue("@Key", songHash);
            ExecuteSQLiteCommand(command, $"Removing {songHash} from whitelist");

            if (Whitelist != null)
                Whitelist.Remove(songHash);
        }

        public List<string> GetWhitelist()
        {
            if (Whitelist != null) 
                return Whitelist;

            var items = new List<string>();
            using (var command = new SQLiteCommand($"SELECT * FROM {WhitelistTableName}", DBMgr.Database))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(reader["Key"].ToString());
                }
            }

            Whitelist = items;
            return Whitelist;
        }

        public void AddToBlacklist(string songHash)
        {
            // Add key to the blacklist table
            using var command = new SQLiteCommand($"INSERT INTO {BlacklistTableName} (Key, Timestamp) VALUES (@Key, @Timestamp)", DBMgr.Database);
            command.Parameters.AddWithValue("@Key", songHash);
            command.Parameters.AddWithValue("@Timestamp", DateTime.Now);
            ExecuteSQLiteCommand(command, $"Adding {songHash} to blacklist");

            if (Blacklist != null) 
                Blacklist.Add(songHash);
        }

        public void RemoveFromBlacklist(string songHash)
        {
            // Remove key from the blacklist table
            using var command = new SQLiteCommand($"DELETE FROM {BlacklistTableName} WHERE Key = @Key", DBMgr.Database);
            command.Parameters.AddWithValue("@Key", songHash);
            ExecuteSQLiteCommand(command, $"Removing {songHash} from blacklist");

            if (Blacklist != null)
                Blacklist.Remove(songHash);
        }

        public List<string> GetBlacklist()
        {
            if (Blacklist != null)
                return Blacklist;

            var items = new List<string>();
            using (var command = new SQLiteCommand($"SELECT * FROM {BlacklistTableName}", DBMgr.Database))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    items.Add(reader["Key"].ToString());
                }
            }

            Blacklist = items;
            return Blacklist;
        }
    }
}
