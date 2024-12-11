using AwayPlayer.Managers;
using AwayPlayer.UI;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using HMUI;
using SiraUtil.Logging;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace AwayPlayer.Utils
{
    internal class ArgParser : IInitializable
    {
        private readonly UnityMainThreadDispatcher Dispatcher;
        private readonly ReplayManager ReplayManager;
        private readonly APIWrapper API;
        private readonly SiraLog SiraLogger;
        private readonly APMenuFloatingScreen FloatingScreen;
        private readonly ScoreListManager SCM;
        public ArgParser(UnityMainThreadDispatcher dispatcher, ReplayManager replayManager, APIWrapper wrapper, SiraLog siraLog, APMenuFloatingScreen floatingScreen, ScoreListManager scoreListManager)
        {
            Dispatcher = dispatcher;
            ReplayManager = replayManager;
            API = wrapper;
            SiraLogger = siraLog;
            FloatingScreen = floatingScreen;
            SCM = scoreListManager;
        }

        public void Initialize()
        {
            var args = Environment.GetCommandLineArgs();

            if (args.Contains("replay"))
            {
                SiraLogger.Debug("Args contain --replay, starting...");
                Dispatcher.EnqueueWithDelay(LoadReplayAsync, 2000);
                if (args.Contains("--autoquit")) ReplayerLauncher.ReplayWasFinishedEvent -= ReplayerLauncher_ReplayWasFinishedEvent;
                return;
            }

            // Made this while in VRC... Ignore the shit code quality
            if (args.Any((x) => x.Contains("autoplay")))
            {
                SiraLogger.Debug("Args contain --autoplay, starting...");
                bool screen = !args.Any((x) => x.Contains("hidescreen"));
                bool instaPlay = args.Any((x) => x.Contains("instaplay"));

                Dispatcher.EnqueueWithDelay(SelectSolo, 3000);
                if (screen) Dispatcher.EnqueueWithDelay(() => FloatingScreen.Visible = true, 4000);
                if (SCM.IsReady) Dispatcher.EnqueueWithDelay(() => ReplayManager.SetupWithOverrides(screen, instaPlay), 5000);
                else
                {
                    void start()
                    {
                        Dispatcher.EnqueueWithDelay(() => ReplayManager.SetupWithOverrides(screen, instaPlay), 5000);
                        SCM.OnScorelistUpdated -= start;
                    }
                    SCM.OnScorelistUpdated += start;
                }
                return;
            }
        }

        private void ReplayerLauncher_ReplayWasFinishedEvent(ReplayLaunchData data)
        {
            Dispatcher.EnqueueWithDelay(() => Application.Quit(0), 5000);
        }

        private async void LoadReplayAsync()
        {
            var args = Environment.GetCommandLineArgs();
            var url = args[Array.IndexOf(args, "--replay") + 1];

            SiraLogger.Debug($"Preparing replay from {url}");

            if (!ReplayDecoder.TryDecodeReplay(await API.GetReplayDataAsync(url), out Replay replay))
            {
                SiraLogger.Error("Failed to load replay!");
                return;
            }

            if (!await ReplayerMenuLoader.Instance.CanLaunchReplay(replay.info))
            {
                SiraLogger.Warn("Failed to load replay! Trying to download...");

                var beatsaver = new BeatSaver("ArgReplayPlayer", new Version(0, 0, 1));
                var beatmap = await beatsaver.BeatmapByHash(replay.info.hash);
                var beatmapZip = await beatmap.LatestVersion.DownloadZIP();
                await ExtractZipAsync(beatmapZip, Path.Combine(Application.dataPath, "CustomLevels"), FolderNameForBeatsaverMap(beatmap), true);
                Dispatcher.EnqueueWithDelay(() => SongCore.Loader.Instance.RefreshSongs(false), 1000);
                Dispatcher.EnqueueWithDelay(LoadReplayAsync, 2000);
                return;
            }

            var levelId = $"custom_level_{replay.info.hash.ToUpper()}";

            Dispatcher.EnqueueWithDelay(SelectSolo, 500);
            Dispatcher.EnqueueWithDelay(() => ReplayManager.ShowLevelPreview(levelId, replay.info.mode, replay.info.difficulty), 1000);
            Dispatcher.EnqueueWithDelay(() => ReplayManager.StartReplayAsync(replay), 2000);
            ReplayerLauncher.ReplayWasFinishedEvent += ReplayerLauncher_ReplayWasFinishedEvent;
        }

        private void SelectSolo()
        {
            // Its safe to assume the game is on the main screen, so no need to check, if this fails, the replay will still start, so no issue there
            (GameObject.Find("SoloButton") ?? GameObject.Find("Wrapper/BeatmapWithModifiers/BeatmapSelection/EditButton"))?.GetComponent<NoTransitionsButton>()?.onClick.Invoke();
        }

        // Stolen from PlaylistManager - Thank you :)
        private string FolderNameForBeatsaverMap(Beatmap song)
        {
            // A workaround for the max path issue and long folder names
            var longFolderName = song.ID + " (" + song.Metadata.SongName + " - " + song.Metadata.LevelAuthorName;
            return longFolderName + ")";
        }
        private async Task ExtractZipAsync(byte[] zip, string customSongsPath, string songName, bool overwrite = false)
        {
            Stream zipStream = new MemoryStream(zip);
            try
            {
                using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);
                var basePath = "";
                basePath = string.Join("", songName.Split(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()));
                var path = Path.Combine(customSongsPath, basePath);

                if (!overwrite && Directory.Exists(path))
                {
                    var pathNum = 1;
                    while (Directory.Exists(path + $" ({pathNum})")) ++pathNum;
                    path += $" ({pathNum})";
                }

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                await Task.Run(() =>
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (!string.IsNullOrWhiteSpace(entry.Name) && entry.Name == entry.FullName)
                        {
                            var entryPath = Path.Combine(path, entry.Name); // Name instead of FullName for better security and because song zips don't have nested directories anyway
                            if (overwrite || !File.Exists(entryPath)) // Either we're overwriting or there's no existing file
                                ZipFileExtensions.ExtractToFile(entry, entryPath, overwrite);
                        }
                    }
                }).ConfigureAwait(false);
                archive.Dispose();
            }
            catch (Exception e)
            {
                SiraLogger.Error($"Unable to extract ZIP! Exception: {e}");
                return;
            }
            zipStream.Close();
        }
    }
}
