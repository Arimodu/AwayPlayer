namespace AwayPlayer.Models
{
    public class ScoreFilterSettings
    {
        public bool FavoritesOnly { get; set; }
        public string Playlist = "None";
        public HMD HMD { get; set; }
        public Controller Controller { get; set; }

        public ScoreFilterSettings(bool favoritesOnly, string playlist, HMD hMD, Controller controller)
        {
            FavoritesOnly = favoritesOnly;
            Playlist = playlist;
            HMD = hMD;
            Controller = controller;
        }

        public override string ToString() => $"FavoritesOnly: {FavoritesOnly}\nPlaylist: {Playlist}\nHMD: {HMD}\nController: {Controller}";
    }

    public enum HMD
    {
        Ignore = -1,
        Unknown = 0,
        Rift = 1,
        RiftS = 16,
        Quest = 32,
        Quest2 = 256,
        Vive = 2,
        VivePro = 4,
        ViveCosmos = 128,
        Wmr = 8,

        PicoNeo3 = 33,
        PicoNeo2 = 34,
        VivePro2 = 35,
        ViveElite = 36,
        Miramar = 37,
        Pimax8k = 38,
        Pimax5k = 39,
        PimaxArtisan = 40,
        HpReverb = 41,
        SamsungWmr = 42,
        QiyuDream = 43,
        Disco = 44,
        LenovoExplorer = 45,
        AcerWmr = 46,
        ViveFocus = 47,
        Arpara = 48,
        DellVisor = 49,
        E3 = 50,
        ViveDvt = 51,
        Glasses20 = 52,
        Hedy = 53,
        Vaporeon = 54,
        Huaweivr = 55,
        AsusWmr = 56,
        Cloudxr = 57,
        Vridge = 58,
        Medion = 59,
        PicoNeo4 = 60,
        QuestPro = 61,
        PimaxCrystal = 62,
        e4 = 63,
        Index = 64,
        Controllable = 65
    }

    public enum Controller
    {
        Ignore = -1,
        Unknown = 0,
        Oculustouch = 1,
        Oculustouch2 = 16,
        Quest2 = 256,
        Vive = 2,

        VivePro = 4,
        Wmr = 8,
        Odyssey = 9,
        HpMotion = 10,

        PicoNeo3 = 33,
        PicoNeo2 = 34,
        VivePro2 = 35,
        Miramar = 37,
        Disco = 44,
        QuestPro = 61,
        ViveTracker = 62,
        ViveTracker2 = 63,
        Knuckles = 64,
        Nolo = 65,
        Picophoenix = 66,
        Hands = 67,
        ViveTracker3 = 68,
        Pimax = 69,
        Huawei = 70,
        Polaris = 71,
        Tundra = 72,
        Cry = 73,
        e4 = 74,
        Gamepad = 75,
        Joycon = 76,
        Steamdeck = 77,
        ViveCosmos = 128,
    }

    public enum DuplicateReplayPolicy
    {
        Allow,
        Prevent,
        Strict
    }
}