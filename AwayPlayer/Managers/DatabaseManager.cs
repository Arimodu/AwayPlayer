using SiraUtil.Logging;
using System;
using System.Data.SQLite;
using System.IO;
using Zenject;

namespace AwayPlayer.Managers
{
    public class DatabaseManager : IInitializable, IDisposable
    {
        internal SQLiteConnection Database;

        [Inject]
        private readonly SiraLog Log;

        public void Initialize()
        {
            Log.Debug("Initializing database manager...");
            var dbPath = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "AwayPlayer_Data.sqlite");

            Database = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            Database.Open();
            Log.Debug("Database manager ready...");
        }

        public void Dispose()
        {
            Database.Close();
        }
    }
}
