using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AwayPlayer.Models
{
    internal class Score
    {
        [JsonProperty("id")]
        public int? Id { get; private set; }

        [JsonProperty("baseScore")]
        public long? BaseScore { get; private set; }

        [JsonProperty("modifiedScore")]
        public long? ModifiedScore { get; private set; }

        [JsonProperty("accuracy")]
        public double? Accuracy { get; private set; }

        [JsonProperty("playerId")]
        public string PlayerId { get; private set; }

        [JsonProperty("pp")]
        public double? Pp { get; private set; }

        [JsonProperty("bonusPp")]
        public double? BonusPp { get; private set; }

        [JsonProperty("passPP")]
        public double? PassPP { get; private set; }

        [JsonProperty("accPP")]
        public double? AccPP { get; private set; }

        [JsonProperty("techPP")]
        public double? TechPP { get; private set; }

        [JsonProperty("rank")]
        public int? Rank { get; private set; }

        [JsonProperty("replay")]
        public string Replay { get; private set; }

        [JsonProperty("modifiers")]
        public string Modifiers { get; private set; }

        [JsonProperty("missedNotes")]
        public int? MissedNotes { get; private set; }

        [JsonProperty("bombCuts")]
        public int? BombCuts { get; private set; }

        [JsonProperty("wallsHit")]
        public int? WallsHit { get; private set; }

        [JsonProperty("pauses")]
        public int? Pauses { get; private set; }

        [JsonProperty("fullCombo")]
        public bool? FullCombo { get; private set; }

        [JsonProperty("maxCombo")]
        public int? MaxCombo { get; private set; }

        [JsonProperty("maxStreak")]
        public int? MaxStreak { get; private set; }

        [JsonProperty("hmd")]
        public int? Hmd { get; private set; }

        [JsonProperty("controller")]
        public int? Controller { get; private set; }

        [JsonProperty("leaderboardId")]
        public string LeaderboardId { get; private set; }

        [JsonProperty("replaysWatched")]
        public int? ReplaysWatched { get; private set; }

        [JsonProperty("playCount")]
        public int? PlayCount { get; private set; }

        public Song Song { get; set; }

        public Difficulty Difficulty { get; set; }

        public override string ToString()
        {
            return $"Score ID: {Id}, Player ID: {PlayerId}, Accuracy: {Accuracy}, PP: {Pp}";
        }
    }
}
