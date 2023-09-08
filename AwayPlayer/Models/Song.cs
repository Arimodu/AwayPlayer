using Newtonsoft.Json;

namespace AwayPlayer.Models
{
    public class Song
    {
        [JsonProperty("id")]
        public string Id { get; private set; }

        [JsonProperty("hash")]
        public string Hash { get; private set; }

        [JsonProperty("name")]
        public string Name { get; private set; }

        [JsonProperty("subName")]
        public string SubName { get; private set; }

        [JsonProperty("author")]
        public string Author { get; private set; }

        [JsonProperty("mapper")]
        public string Mapper { get; private set; }

        [JsonProperty("coverImage")]
        public string CoverImage { get; private set; }

        [JsonProperty("fullCoverImage")]
        public string FullCoverImage { get; private set; }

        [JsonProperty("downloadUrl")]
        public string DownloadUrl { get; private set; }

        [JsonProperty("bpm")]
        public double? Bpm { get; private set; }

        [JsonProperty("duration")]
        public int? Duration { get; private set; }

        [JsonProperty("tags")]
        public string Tags { get; private set; }

        [JsonProperty("uploadTime")]
        public long? UploadTime { get; private set; }

        public override string ToString()
        {
            return $"Song {Name} by {Author} mapped by {Mapper} (ID: {Id})";
        }
    }
}