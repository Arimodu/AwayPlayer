using AwayPlayer.Converters;
using AwayPlayer.Models;
using IPA.Config.Stores.Attributes;
using System;

namespace AwayPlayer
{
    public class APConfig
    {
        public virtual bool DisableDeveloperNameText { get; set; } = false;
        public string Playlist { get; set; } = "None";
        public bool FavoritesOnly { get; set; } = false;
        public bool WhitelistEnable { get; set; } = true;
        public bool BlacklistEnable { get; set; } = true;
        public int StartDelay { get; set; } = 3;

        [UseConverter(typeof(StringEnumConverter<DuplicateReplayPolicy>))]
        public virtual DuplicateReplayPolicy DuplicateReplayPolicy { get; set; } = DuplicateReplayPolicy.Prevent;

        [UseConverter(typeof(StringEnumConverter<Controller>))]
        public Controller Controller { get; set; } = Controller.Ignore;

        [UseConverter(typeof(StringEnumConverter<HMD>))]
        public HMD HMD { get; set; } = HMD.Ignore;

        public event Action<APConfig> OnChanged;

        public virtual void Changed()
        {
            OnChanged?.Invoke(this);
        }
    }
}