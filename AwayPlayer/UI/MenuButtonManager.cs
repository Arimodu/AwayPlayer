﻿using AwayPlayer.Managers;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using System.Linq;
using UnityEngine;
using Zenject;

#pragma warning disable CS0649 // Value is never assigend to - We have zenject
namespace AwayPlayer.UI
{
    internal class MenuButtonManager : BSMLAutomaticViewController, IInitializable
    {
        private const string AFK_BUTTON = "" +
            "<bg id='root'>" +
            "<button id='afk-button' text='AFK Mode' anchor-pos-x='122' anchor-pos-y='-2' on-click='afk-click'/>" +
            "</bg>";
        private const string BLACKLIST_BUTTON = "" +
            "<bg id='root'>" +
            "<button id='blacklist-button' text='B' active='~blacklist-button-active' hover-hint='Adds the current song to the AwayPlayer backlist' anchor-pos-x='56' anchor-pos-y='-3' pref-width='8' pref-height='8' on-click='blacklist-click'/>" +
            "</bg>";
        private const string PRIMARY_BLACKLIST_BUTTON = "" +
            "<bg id='root'>" +
            "<primary-button id='blacklist-button' active='~primary-blacklist-button-active' text='B' hover-hint='Removes the current song to the AwayPlayer backlist' anchor-pos-x='26' anchor-pos-y='0' pref-width='8' pref-height='8' on-click='primary-blacklist-click'/>" +
            "</bg>";
        private const string WHITELIST_BUTTON = "" +
            "<bg id='root'>" +
            "<button id='whitelist-button' text='W' active='~whitelist-button-active' hover-hint='Adds the current song to the AwayPlayer whitelist' anchor-pos-x='48' anchor-pos-y='-3' pref-width='8' pref-height='8' on-click='whitelist-click'/>" +
            "</bg>";
        private const string PRIMARY_WHITELIST_BUTTON = "" +
            "<bg id='root'>" +
            "<primary-button id='whitelist-button' active='~primary-whitelist-button-active' text='W' hover-hint='Removes the current song to the AwayPlayer whitelist' anchor-pos-x='18' anchor-pos-y='0' pref-width='8' pref-height='8' on-click='primary-whitelist-click'/>" +
            "</bg>";

        private bool _blacklistButtonActive = true;
        private bool _whitelistButtonActive = true;
        private bool _primaryBlacklistButtonActive = false;
        private bool _primaryWhitelistButtonActive = false;

        [UIValue("blacklist-button-active")]
        public bool BlacklistButtonActive
        {
            get => _blacklistButtonActive;
            set
            {
                _blacklistButtonActive = value;
                NotifyPropertyChanged();
                if (value) PrimaryBlacklistButtonActive = false;
            }
        }

        [UIValue("primary-blacklist-button-active")]
        public bool PrimaryBlacklistButtonActive
        {
            get => _primaryBlacklistButtonActive;
            set
            {
                _primaryBlacklistButtonActive = value;
                NotifyPropertyChanged();
                if (value) BlacklistButtonActive = false;
            }
        }

        [UIValue("whitelist-button-active")]
        public bool WhitelistButtonActive
        {
            get => _whitelistButtonActive;
            set
            {
                _whitelistButtonActive = value;
                NotifyPropertyChanged();
                if (value) PrimaryWhitelistButtonActive = false;
            }
        }

        [UIValue("primary-whitelist-button-active")]
        public bool PrimaryWhitelistButtonActive
        {
            get => _primaryWhitelistButtonActive;
            set
            {
                _primaryWhitelistButtonActive = value;
                NotifyPropertyChanged();
                if (value) WhitelistButtonActive = false;
            }
        }

        [Inject]
        private readonly ReplayManager _replayManager;

        [Inject]
        private readonly APMenuFloatingScreen _menuFloatingScreen;

        [Inject]
        private readonly APConfig _config;

        [Inject]
        private readonly WhitelistBlacklistManager WBMgr;

        [Inject]
        private readonly ScoreListManager SLM;

        public void Initialize()
        {
            BSMLParser.Instance.Parse(AFK_BUTTON, Resources.FindObjectsOfTypeAll<LevelSelectionNavigationController>().First().gameObject, this);
            var standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First(); // Stolen from song core, thank you ;)
            if (_config.BlacklistEnable)
            {
                BSMLParser.Instance.Parse(BLACKLIST_BUTTON, standardLevel.transform.Find("LevelDetail").gameObject, this);
                BSMLParser.Instance.Parse(PRIMARY_BLACKLIST_BUTTON, standardLevel.transform.Find("LevelDetail").gameObject, this);
            }
            if (_config.WhitelistEnable)
            {
                BSMLParser.Instance.Parse(WHITELIST_BUTTON, standardLevel.transform.Find("LevelDetail").gameObject, this);
                BSMLParser.Instance.Parse(PRIMARY_WHITELIST_BUTTON, standardLevel.transform.Find("LevelDetail").gameObject, this);
            }

            var controller = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            controller.didChangeDifficultyBeatmapEvent += OnDifficultyChanged;
            controller.didChangeContentEvent += OnContentChanged;
        }

        private void OnContentChanged(StandardLevelDetailViewController controller, StandardLevelDetailViewController.ContentType type)
        {
            if (type == StandardLevelDetailViewController.ContentType.OwnedAndReady) UpdateButtons(controller.beatmapLevel.levelID.Remove(0, 13).ToUpper());
        }

        private void OnDifficultyChanged(StandardLevelDetailViewController controller)
        {
            UpdateButtons(controller.beatmapLevel.levelID.Remove(0, 13).ToUpper());
        }

        private void UpdateButtons(string selectedSong)
        {
            var blacklist = WBMgr.GetBlacklist();
            var whitelist = WBMgr.GetWhitelist();

            if (blacklist.Contains(selectedSong))
            {
                PrimaryBlacklistButtonActive = true;
                return;
            }

            if (whitelist.Contains(selectedSong))
            {
                PrimaryWhitelistButtonActive = true;
                return;
            }

            WhitelistButtonActive = true;
            BlacklistButtonActive = true;
        }

        [UIAction("afk-click")]
        public void AFKClick()
        {
            if (_replayManager.Enabled)
            {
                _replayManager.Enabled = false;
                _menuFloatingScreen.Visible = false;
                return;
            }
            _replayManager.Setup();
            _menuFloatingScreen.Visible = true;
        }

        [UIAction("blacklist-click")]
        public void BlacklistClick()
        {
            var selectedSong = GetSelectedSongHash();

            if (WBMgr.GetWhitelist().Contains(selectedSong))
            {
                WBMgr.RemoveFromWhitelist(selectedSong);
                WhitelistButtonActive = true;
            }

            WBMgr.AddToBlacklist(selectedSong);
            PrimaryBlacklistButtonActive = true;

            SLM.ForceReload();

            if (_replayManager.Enabled && _replayManager.CurrentScore.Song.Hash == selectedSong)
            {
                _replayManager.SkipCurrentSelection();
            }
        }

        [UIAction("primary-blacklist-click")]
        public void PrimaryBlacklistClick()
        {
            var selectedSong = GetSelectedSongHash();
            WBMgr.RemoveFromBlacklist(selectedSong);
            BlacklistButtonActive = true;

            SLM.ForceReload();
        }

        [UIAction("whitelist-click")]
        public void WhitelistClick()
        {
            var selectedSong = GetSelectedSongHash();

            if (WBMgr.GetBlacklist().Contains(selectedSong))
            {
                WBMgr.RemoveFromBlacklist(selectedSong);
                BlacklistButtonActive = true;
            }

            WBMgr.AddToWhitelist(selectedSong);
            PrimaryWhitelistButtonActive = true;

            SLM.ForceReload();
        }

        [UIAction("primary-whitelist-click")]
        public void PrimaryWhitelistClick()
        {
            var selectedSong = GetSelectedSongHash();
            WBMgr.RemoveFromWhitelist(selectedSong);
            WhitelistButtonActive = true;

            SLM.ForceReload();
        }

        private string GetSelectedSongHash()
        {
            var standardLevel = Resources.FindObjectsOfTypeAll<StandardLevelDetailViewController>().First();
            return standardLevel.beatmapLevel.levelID.Remove(0, 13).ToUpper();
        }
    }
}
