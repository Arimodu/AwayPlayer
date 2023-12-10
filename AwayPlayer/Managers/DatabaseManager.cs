using System;
using System.Data.SQLite;
using System.IO;
using Zenject;

namespace AwayPlayer.Managers
{
    public class DatabaseManager : IInitializable, IDisposable
    {
        internal SQLiteConnection Database;

        public void Initialize()
        {
            var dbPath = Path.Combine(IPA.Utilities.UnityGame.UserDataPath, "AwayPlayer_Data.sqlite");

            Database = new SQLiteConnection($"Data Source={dbPath};Version=3;");
            Database.Open();
        }

        public void Dispose()
        {
            Database.Close();
        }
    }
}
