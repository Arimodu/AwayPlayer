using AwayPlayer.Models;
using BeatLeader.Models;
using BeatLeader.Models.Replay;
using BeatLeader.Replayer;
using BeatSaverSharp;
using BeatSaverSharp.Models;
using HMUI;
using SiraUtil.Logging;
using SiraUtil.Web;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace AwayPlayer.Utils
{
    internal class ArgReplayStarter : IInitializable
    {
        private readonly UnityMainThreadDispatcher Dispatcher;
        private readonly ReplayManager ReplayManager;
        private readonly APIWrapper API;
        private readonly SiraLog SiraLogger;
        private readonly IHttpService HttpService;
        public ArgReplayStarter(UnityMainThreadDispatcher dispatcher, ReplayManager replayManager, APIWrapper wrapper, SiraLog siraLog, IHttpService httpService)
        {
            Dispatcher = dispatcher;
            ReplayManager = replayManager;
            API = wrapper;
            SiraLogger = siraLog;
            HttpService = httpService;
        }

        public void Initialize()
        {
            //SiraLogger.Notice("Init on ArgReplayStarter");
            Dispatcher.EnqueueWithDelay(LoadReplayAsync, 2000);
        }

        private void ReplayerLauncher_ReplayWasFinishedEvent(ReplayLaunchData data)
        {
            Dispatcher.EnqueueWithDelay(() => Application.Quit(0), 5000);
        }

        private async void LoadReplayAsync()
        {
            //SiraLogger.Notice("LoadReplayAsync on ArgReplayStarter");
            var args = Environment.GetCommandLineArgs();

            if (!args.Contains("--replay"))
            {
                SiraLogger.Debug("Args dont contain --replay, exitting...");
                ReplayerLauncher.ReplayWasFinishedEvent -= ReplayerLauncher_ReplayWasFinishedEvent;
                return;
            }

            var url = args[args.IndexOf("--replay") + 1];

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
            return longFolderName.Truncate(49, true) + ")";
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
