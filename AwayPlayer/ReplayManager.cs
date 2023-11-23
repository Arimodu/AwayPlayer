using AwayPlayer.Managers;
using AwayPlayer.Models;
using AwayPlayer.UI;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatLeader.Utils;
using ModestTree;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = System.Random;
using Score = AwayPlayer.Models.Score;
using static BeatLeader.Utils.BeatLeaderConstants;

namespace AwayPlayer
{
    internal class ReplayManager : IInitializable
    {
        private readonly SiraLog Log;
        private readonly APIWrapper API;
        private readonly APConfig Config;
        private readonly ScoreListManager ScoreListManager;
        private readonly UnityMainThreadDispatcher Dispatcher;
        private readonly BeatmapLevelsModel LevelsModel;
        private readonly Random rnd = new Random();
        internal protected APMenuFloatingScreen FloatingScreen;
        private bool _enabled = false;
        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (!value) Dispatcher.TryCancelDelayedTask(StartJobID);
                _enabled = value;
            }
        }
        public bool IsPlaying { get; private set; }
        public bool IsLoaded { get; private set; }

        public Queue<Score> Scores { get; private set; }
        public Score CurrentScore { get; set; }
        public Replay LoadedReplay { get; private set; }

        public Guid StartJobID { get; private set; }

        public ReplayManager(SiraLog siraLog, APIWrapper wrapper, ScoreListManager scoreListManager, UnityMainThreadDispatcher dispatcher, BeatmapLevelsModel levelsModel, APConfig config)
        {
            Log = siraLog;
            API = wrapper;
            ScoreListManager = scoreListManager;
            Dispatcher = dispatcher;
            LevelsModel = levelsModel;
            Config = config;
            Scores = new Queue<Score>(); // Just init to empty queue
#if DEBUG
            Log.DebugMode = true;
#endif
        }

        public void Initialize()
        {
            ReplayerLauncher.ReplayWasFinishedEvent += ReplayerLauncher_ReplayWasFinishedEvent;
        }

        private void ReplayerLauncher_ReplayWasFinishedEvent(ReplayLaunchData data)
        {
            if (!Enabled || !IsPlaying) return; // Generally, is playing shouldnt matter, but eh, can check it anyway, since it doesnt matter

            Dispatcher.Enqueue(() => IsPlaying = false); // idk, its not getting set, so lets try this

            SelectRandomReplay();

            Dispatcher.EnqueueWithDelay(PrepareReplayAsync, 2000);
        }

        public void Setup()
        {
            SelectRandomReplay();
            Dispatcher.Enqueue(PrepareReplayAsync);
        }

        public void SelectRandomReplay()
        {
            Dispatcher.Enqueue(() => IsLoaded = false); // This fix seems to have worked for IsPlaying so it should work for IsLoaded too
            if (Scores.IsEmpty())
            {
                switch (Config.DuplicateReplayPolicy)
                {
                    case DuplicateReplayPolicy.Allow:
                        CurrentScore = ScoreListManager.FilteredScores[rnd.Next(ScoreListManager.FilteredScores.Length)];
                        return;
                    case DuplicateReplayPolicy.Prevent:
                        Scores = new Queue<Score>(ScoreListManager.FilteredScores.Shuffle(rnd));
                        break;
                    case DuplicateReplayPolicy.Strict:
                        Enabled = false;
                        // Lets just... Not implement this yet....
                        return;
                    default:
                        throw new InvalidOperationException(); // What would even cause this.... Well I dont care, InvalidOperationException it is
                }
            }
            CurrentScore = Scores.Dequeue();
        }

        public void SkipCurrentSelection()
        {
            if (!IsPlaying)
            {
                Dispatcher.CancelDelayedTask(StartJobID);

                IsLoaded = false;

                SelectRandomReplay();
                Dispatcher.Enqueue(PrepareReplayAsync);
            }

            // If we are already in game, dont do anything
        }

        public async Task PrepareReplayAsync()
        {
            if (IsLoaded) return;
            if (ReplayerCache.TryReadReplay((int)CurrentScore.Id, out Replay replay))
            {
                if (!await ReplayerMenuLoader.Instance.CanLaunchReplay(replay.info))
                {
                    Log.Warn($"Failed to load replay for {CurrentScore.Song.Name}");
                    SelectRandomReplay();
                    await PrepareReplayAsync();
                    return;
                }
            }
            else if (ReplayDecoder.TryDecodeReplay(await API.GetReplayDataAsync(CurrentScore.Replay), out replay))
            {
                if (!await ReplayerMenuLoader.Instance.CanLaunchReplay(replay.info))
                {
                    Log.Warn($"Failed to load replay for {CurrentScore.Song.Name}");
                    SelectRandomReplay();
                    await PrepareReplayAsync();
                    return;
                }
            }

            LoadedReplay = replay;
            IsLoaded = true;

            lock (this)
            {
                var levelId = $"custom_level_{CurrentScore.Song.Hash.ToUpper()}";
                Dispatcher.Enqueue(() => ShowLevelPreview(levelId, CurrentScore.Difficulty.ModeName, CurrentScore.Difficulty.Name));

                // Try to cancel in case it is still running
                Dispatcher.TryCancelDelayedTask(StartJobID);

                // Save the job Id in case we want to cancel
                StartJobID = Dispatcher.EnqueueWithDelay(StartReplayAsync, Config.StartDelay * 1000, (remaining) =>
                {
                    FloatingScreen.Timeout = remaining.ToString();
                }); 
            }
        }

        public void ShowLevelPreview(string levelId, string characteristicName, string difficultyName)
        {
            var selectionController = GameObject.Find("LevelSelectionNavigationController").GetComponent<LevelSelectionNavigationController>();

            if (selectionController == null)
            {
                Log.Error("Could not find LevelSelectionNavigationController! Cannot show preview!");
                return;
            }

            var beatmapPack = LevelsModel.GetLevelPackForLevelId(levelId);
            var preview = LevelsModel.GetLevelPreviewForLevelId(levelId);
            var beatmapDifficultySet = LevelsModel.GetBeatmapLevelIfLoaded(levelId).beatmapLevelData.GetDifficultyBeatmapSet(characteristicName);
            var beatmapCharacteristic = beatmapDifficultySet.beatmapCharacteristic;
            var beatmapDifficulty = beatmapDifficultySet.difficultyBeatmaps.Where((x) => x.difficulty == (BeatmapDifficulty)Enum.Parse(typeof(BeatmapDifficulty), difficultyName)).FirstOrDefault();

            try
            {
                try
                {
                    ForceSelectFilterCategory(SelectLevelCategoryViewController.LevelCategory.All);
                }
                catch (System.NullReferenceException)
                {
                    Log.Error("Could not force select LevelCategory.All. Trying to show level preview anyway...");
                }

                selectionController.Setup(SongPackMask.all, BeatmapDifficultyMask.All, new BeatmapCharacteristicSO[0], false, false, "Play", beatmapPack, SelectLevelCategoryViewController.LevelCategory.All, preview, true);
                var characteristicIndex = selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapCharacteristicSegmentedControlController.
                    _beatmapCharacteristics.IndexOf(beatmapCharacteristic);

                var characteristicSegmentedControl = selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapCharacteristicSegmentedControlController.
                    _segmentedControl;

                characteristicSegmentedControl.SelectCellWithNumber(characteristicIndex);

                selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapCharacteristicSegmentedControlController.
                    HandleDifficultySegmentedControlDidSelectCell(characteristicSegmentedControl, characteristicIndex);

                var difficultyIndex = selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapDifficultySegmentedControlController.
                    GetClosestDifficultyIndex(beatmapDifficulty.difficulty);

                var difficultySegmentedControl = selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapDifficultySegmentedControlController.
                    _difficultySegmentedControl;

                difficultySegmentedControl.SelectCellWithNumber(difficultyIndex);

                selectionController.
                    _levelCollectionNavigationController.
                    _levelDetailViewController.
                    _standardLevelDetailView.
                    _beatmapDifficultySegmentedControlController.
                    HandleDifficultySegmentedControlDidSelectCell(difficultySegmentedControl, difficultyIndex);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to show preview for {levelId}...");
                Log.Critical(e);
            }
        }

        private void ForceSelectFilterCategory(SelectLevelCategoryViewController.LevelCategory levelCategory)
        {
            try
            {
                var selectionController = GameObject.Find("LevelSelectionNavigationController").GetComponent<LevelSelectionNavigationController>();

                var navController = selectionController._levelFilteringNavigationController;

                if (navController.selectedLevelCategory != levelCategory)
                {
                    var categorySelector = selectionController._levelFilteringNavigationController._selectLevelCategoryViewController;
                    if (categorySelector != null)
                    {
                        var iconSegmentControl = categorySelector._levelFilterCategoryIconSegmentedControl;
                        var categoryInfos = categorySelector._levelCategoryInfos;
                        var index = categoryInfos.Select(x => x.levelCategory).ToArray().IndexOf(levelCategory);

                        iconSegmentControl.SelectCellWithNumber(index);
                        categorySelector.LevelFilterCategoryIconSegmentedControlDidSelectCell(iconSegmentControl, index);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error("Failed to force category selection");
                Log.Critical(e);
            }
        }

        public async Task StartReplayAsync()
        {
            Log.Debug($"Starting replay...\nEnabled: {Enabled}\nIsPlaying: {IsPlaying}\nIsLoaded: {IsLoaded}");
            if (!Enabled || IsPlaying || !IsLoaded) return;
            await StartReplayAsync(LoadedReplay);
        }
        public async Task StartReplayAsync(Replay replay)
        {
            IsPlaying = true;
            var player = await WebUtils.SendAndDeserializeAsync<Player>(BEATLEADER_API_URL + "/player/" + replay.info.playerID);
            await ReplayerMenuLoader.Instance.StartReplayAsync(replay, player, ReplayerSettings.UserSettings);
        }
    }
}
