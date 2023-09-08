using AwayPlayer.UI;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using SiraUtil.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;
using Random = System.Random;
using Score = AwayPlayer.Models.Score;

namespace AwayPlayer
{
    internal class ReplayManager : IInitializable
    {
        private readonly SiraLog Log;
        private readonly APIWrapper API;
        private readonly UnityMainThreadDispatcher Dispatcher;
        private readonly BeatmapLevelsModel LevelsModel;
        private readonly Random rnd = new Random();
        internal protected MenuFloatingScreen FloatingScreen;
        public bool Enabled = false;
        public bool IsPlaying = false;

        public List<Score> Scores { get; private set; }
        public List<Score> AllScores { get; private set; }

        public Score CurrentScore { get; set; }
        public Replay LoadedReplay { get; private set; }
        public bool IsLoaded { get; private set; }

        public ReplayManager(SiraLog siraLog, APIWrapper wrapper, UnityMainThreadDispatcher dispatcher, BeatmapLevelsModel levelsModel)
        {
            Log = siraLog;
            API = wrapper;
            Dispatcher = dispatcher;
            LevelsModel = levelsModel;
#if DEBUG
            Log.DebugMode = true;
#endif
        }

        public async void Initialize()
        {
            await GetAndFilterReplays();

            ReplayerLauncher.ReplayWasFinishedEvent += ReplayerLauncher_ReplayWasFinishedEvent;
        }

        private async Task GetAndFilterReplays()
        {
            Log.Info("Starting replay fetch...");

            AllScores = await API.GetReplayListAsync();

            Scores = AllScores.Where((score) => score.Hmd == 64).ToList(); // For the time being all scores where the headset is index. Yea I dont want to look at my own quest replays, dont judge me

            Log.Info($"Replay fetch finished\nTotal fetched: {AllScores.Count}\nTotal filtered: {Scores.Count}");
        }

        private void ReplayerLauncher_ReplayWasFinishedEvent(ReplayLaunchData data)
        {
            IsPlaying = false;

            SelectRandomReplay();

            Dispatcher.EnqueueWithDelay(PrepareReplayAsync, 3000);
        }

        public void Setup()
        {
            SelectRandomReplay();
            Dispatcher.Enqueue(PrepareReplayAsync);
        }

        public void SelectRandomReplay()
        {
            CurrentScore = Scores[rnd.Next(Scores.Count)];
        }

        public async Task PrepareReplayAsync()
        {
            if (ReplayerCache.TryReadReplay((int)CurrentScore.Id, out Replay replay))
            {
                if (!await ReplayerMenuLoader.Instance.CanLaunchReplay(replay.info))
                {
                    Log.Warn($"Failed to load replay for {CurrentScore.Song.Name}");
                    Scores.Remove(CurrentScore);
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
                    Scores.Remove(CurrentScore);
                    SelectRandomReplay();
                    await PrepareReplayAsync();
                    return;
                }
            }

            LoadedReplay = replay;
            IsLoaded = true;

            var levelId = $"custom_level_{CurrentScore.Song.Hash}";
            Dispatcher.Enqueue(() => ShowLevelPreview(levelId));

            Dispatcher.EnqueueWithDelay(StartReplayAsync, 10000, (remaining) =>
            {
                Log.Info($"{remaining} seconds remaining");
                FloatingScreen.Timeout = remaining.ToString();
            });
        }

        private void ShowLevelPreview(string levelId)
        {
            var selectionController = GameObject.Find("LevelSelectionNavigationController").GetComponent<LevelSelectionNavigationController>();

            if (selectionController == null)
            {
                Log.Error("Could not find LevelSelectionNavigationController! Cannot show preview!");
                return;
            }

            var beatmapPack = LevelsModel.GetLevelPackForLevelId(levelId);
            var preview = LevelsModel.GetLevelPreviewForLevelId(levelId);

            bool isError = false;
            int errorCounter = 0;

            do
            {
                try
                {
                    var navController = selectionController._levelFilteringNavigationController;

                    try
                    {
                        if (navController.selectedLevelCategory != SelectLevelCategoryViewController.LevelCategory.All)
                        {
                            var categorySelector = selectionController._levelFilteringNavigationController._selectLevelCategoryViewController;
                            if (categorySelector != null)
                            {
                                var iconSegmentControl = categorySelector._levelFilterCategoryIconSegmentedControl;
                                var categoryInfos = categorySelector._levelCategoryInfos;
                                var index = categoryInfos.Select(x => x.levelCategory).ToArray().IndexOf(SelectLevelCategoryViewController.LevelCategory.All);

                                iconSegmentControl.SelectCellWithNumber(index);
                                categorySelector.LevelFilterCategoryIconSegmentedControlDidSelectCell(iconSegmentControl, index);
                            }
                        }
                    }
                    catch (System.NullReferenceException)
                    {
                        Log.Error("Could not force select LevelCategory.All. Trying to show level preview anyway...");
                    }

                    selectionController.Setup(SongPackMask.all, BeatmapDifficultyMask.All, new BeatmapCharacteristicSO[0], false, false, "Play", beatmapPack, SelectLevelCategoryViewController.LevelCategory.All, preview, true);
                }
                catch (System.Exception e)
                {
                    isError = true;
                    errorCounter++;
                    Log.Error($"Failed to show preview, retrying... (counter at {errorCounter})");
                    Log.Critical(e);

                }
            } while (isError && errorCounter < 5); // Try as long as error is true and errorCounter is less than 5
        }

        public async Task StartReplayAsync() => await StartReplayAsync(LoadedReplay);
        public async Task StartReplayAsync(Replay replay)
        {
            if (!Enabled || IsPlaying || !IsLoaded) return;
            IsPlaying = true;

            var source = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            try
            {
                await ReplayerMenuLoader.Instance.StartReplayAsync(replay, BeatLeader.DataManager.ProfileManager.Profile, null, source.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Error("Failed to start replay within 10 seconds, retrying from new task...");
                IsPlaying = false;
                Dispatcher.Enqueue(StartReplayAsync);
            }
            finally
            {
                source.Dispose();
            }
        }
    }
}
