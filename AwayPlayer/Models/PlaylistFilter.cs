namespace AwayPlayer.Models
{
    internal class PlaylistFilter
    {
        public string PlaylistName { get; set; }
        public PlaylistFilterType Type { get; set; }

        public PlaylistFilter(string playlistName, PlaylistFilterType type)
        {
            PlaylistName = playlistName;
            Type = type;
        }
    }

    public enum PlaylistFilterType
    {
        Include,
        Exclude
    }
}
