using AwayPlayer.Managers;
using AwayPlayer.Models;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Zenject;

#pragma warning disable IDE0051 // Remove unused private members
#pragma warning disable IDE0044 // Set field readonly
namespace AwayPlayer.UI
{
    [ViewDefinition("AwayPlayer.UI.BSML.MainMenu.bsml")]
    [HotReload(RelativePathToLayout = @"./BSML/MainMenu.bsml")]
    internal class APMainMenu : BSMLAutomaticViewController, IInitializable
    {
        [Inject]
        private readonly ReplayManager _replayManager;

        [Inject]
        private readonly APMenuFloatingScreen _menuFloatingScreen;

        [Inject]
        private readonly ScoreListManager _scoreListManager;

        [Inject]
        private readonly UnityMainThreadDispatcher _unityMainThreadDispatcher;

        [Inject]
        private readonly APConfig Config;

        [UIComponent("TotalFilteredText")]
        private TextMeshProUGUI TotalFilteredText;

        [UIComponent("RefreshLoadingIndicator")]
        private ImageView RefreshLoadingIndicator;

        [UIValue("Playlists")]
        private List<object> Playlists = new List<object> { "None" };

        [UIValue("Playlist")]
        private string Playlist = "None";

        [UIValue("HMDs")]
        private List<object> HMDs = new List<object> { "Ignore" };

        [UIValue("HMD")]
        private string HMD = "Ignore";

        [UIValue("Controllers")]
        private List<object> Controllers = new List<object> { "Ignore" };

        [UIValue("Controller")]
        private string Controller = "Ignore";

        [UIValue("FavoritesOnly")]
        private bool FavoritesOnly = false;

        public void Initialize()
        {
            _scoreListManager.OnScorelistUpdated += ScoreListManager_OnScorelistUpdated;
            _scoreListManager.OnScorelistUpdated += RefreshDropdowns;
            //GameplaySetup.instance.AddTab("AwayPlayer", "AwayPlayer.UI.BSML.StartButtonView.bsml", this);
        }

        private void RefreshDropdowns()
        {
            _scoreListManager.OnScorelistUpdated -= RefreshDropdowns;
            Playlists.Clear();
            Playlists.AddRange(_scoreListManager.GetPlaylistsAsObject());
            HMDs.Clear();
            HMDs.AddRange(_scoreListManager.GetHMDsAsObject());
            Controllers.Clear();
            Controllers.AddRange(_scoreListManager.GetControllersAsObject());
            HMD = Config.HMD.ToString();
            Playlist = Config.Playlist;
            Controller = Config.Controller.ToString();
            FavoritesOnly = Config.FavoritesOnly;
        }

        private void ScoreListManager_OnScorelistUpdated()
        {
            if (wasActivatedBefore)
            {
                TotalFilteredText.text = $"Total: {_scoreListManager.AllScores.Count} : {_scoreListManager.FilteredScores.Length} Filtered";
                RefreshLoadingIndicator.gameObject.SetActive(false);
            }
            else didActivate = (x, y, z) =>
            {
                TotalFilteredText.text = $"Total: {_scoreListManager.AllScores.Count} : {_scoreListManager.FilteredScores.Length} Filtered";
                RefreshLoadingIndicator.gameObject.SetActive(false);
            };
        }

        [UIAction("enable_button")]
        private void EnableButton()
        {
            if (_replayManager.Enabled) return;
            _replayManager.Setup();
            _menuFloatingScreen.Visible = true;
        }

        [UIAction("Reload")]
        internal void Reload()
        {
            RefreshLoadingIndicator.gameObject.SetActive(true);
            Config.Playlist = Playlist;
            Config.HMD = (HMD)Enum.Parse(typeof(HMD), HMD);
            Config.Controller = (Controller)Enum.Parse(typeof(Controller), Controller);
            Config.FavoritesOnly = FavoritesOnly;
            Task.Run(Config.Changed);
        }

        [UIAction("Generate")]
        internal void Generate()
        {
            _scoreListManager.GeneratePlaylists();
        }

        //[UIAction("ByPlaylistChanged")]
        //private void ByPlaylistChanged(string value)
        //{
        //    Config.Playlist = value;
        //}

        //[UIAction("ByHMDChanged")]
        //private void ByHMDChanged(string value)
        //{
        //    Config.HMD = (HMD)Enum.Parse(typeof(HMD), value);
        //}
    }
}
