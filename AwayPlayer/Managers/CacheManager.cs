using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using Zenject;

public class CacheManager<T> : IInitializable, IDisposable where T : class
{
    private SQLiteConnection _database;
    private string _tableName;

    public void Initialize()
    {
        var userInfo = BS_Utils.Gameplay.GetUserInfo.GetPlatformUserModel().GetUserInfo().Result;
        var userId = userInfo.platformUserId;
        var dbPath = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "AwayPlayer_Cache.sqlite");

        _database = new SQLiteConnection($"Data Source={dbPath};Version=3;");
        _database.Open();

        _tableName = $"{typeof(T).Name}Cache_{userId}";

        // Create the table if it doesn't exist
        using var command = new SQLiteCommand($"CREATE TABLE IF NOT EXISTS {_tableName} (Id INTEGER PRIMARY KEY AUTOINCREMENT, Key TEXT UNIQUE, Value BLOB, Timestamp DATETIME)", _database);
        command.ExecuteNonQuery();
    }

    public void AddOrUpdateCache(string leaderboardId, T value)
    {
        var key = leaderboardId;
        var jsonValue = JsonConvert.SerializeObject(value);

        var cacheItem = new CacheItem { Key = key, Value = Encoding.UTF8.GetBytes(jsonValue), Timestamp = DateTime.Now };

        using var command = new SQLiteCommand($"INSERT OR REPLACE INTO {_tableName} (Key, Value, Timestamp) VALUES (@Key, @Value, @Timestamp)", _database);
        command.Parameters.AddWithValue("@Key", cacheItem.Key);
        command.Parameters.AddWithValue("@Value", cacheItem.Value);
        command.Parameters.AddWithValue("@Timestamp", cacheItem.Timestamp);

        command.ExecuteNonQuery();
    }

    public T GetCache(string leaderboardId)
    {
        var key = leaderboardId;

        using var command = new SQLiteCommand($"SELECT * FROM {_tableName} WHERE Key = @Key", _database);
        command.Parameters.AddWithValue("@Key", key);

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var jsonValue = Encoding.UTF8.GetString((byte[])reader["Value"]);
            return JsonConvert.DeserializeObject<T>(jsonValue);
        }

        return null;
    }

    public List<T> GetAllCacheEntries()
    {
        var cacheItems = new List<CacheItem>();

        using (var command = new SQLiteCommand($"SELECT * FROM {_tableName}", _database))
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var cacheItem = new CacheItem
                {
                    Key = reader["Key"].ToString(),
                    Value = (byte[])reader["Value"],
                    Timestamp = Convert.ToDateTime(reader["Timestamp"])
                };

                cacheItems.Add(cacheItem);
            }
        }

        return cacheItems.Select(cacheItem =>
        {
            var jsonValue = Encoding.UTF8.GetString(cacheItem.Value);
            return JsonConvert.DeserializeObject<T>(jsonValue);
        }).ToList();
    }

    public void RemoveCache(string key)
    {
        using var command = new SQLiteCommand($"DELETE FROM {_tableName} WHERE Key = @Key", _database);
        command.Parameters.AddWithValue("@Key", key);
        command.ExecuteNonQuery();
    }

    public void ClearCache()
    {
        using var command = new SQLiteCommand($"DELETE FROM {_tableName}", _database);
        command.ExecuteNonQuery();
    }

    public int CountCacheItems()
    {
        using var command = new SQLiteCommand($"SELECT COUNT(*) FROM {_tableName}", _database);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Dispose()
    {
        _database.Close();
    }
}

public class CacheItem
{
    public string Key { get; set; }
    public byte[] Value { get; set; }
    public DateTime Timestamp { get; set; }
}
