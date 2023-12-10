using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.Tags;
using BeatSaberMarkupLanguage.ViewControllers;
using IPA.Config.Data;
using SiraUtil.Logging;
using System;
using System.IO;
using TMPro;
using Tweening;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#pragma warning disable IDE0051 // Remove unused private members
namespace AwayPlayer.UI
{
    [ViewDefinition("AwayPlayer.UI.BSML.MenuFloatingScreen.bsml")]
    [HotReload(RelativePathToLayout = @"./BSML/MenuFloatingScreen.bsml")]
    internal class APMenuFloatingScreen : BSMLAutomaticViewController, IInitializable
    {
        private FloatingScreen _floatingScreen;
        //private FloatingScreen _floatingButton;

        [Inject]
        private readonly ReplayManager _replayManager;

        [Inject]
        private readonly UnityMainThreadDispatcher _unityMainThreadDispatcher;

        [Inject]
        private readonly APConfig _config;

        [Inject]
        private readonly TimeTweeningManager _timeTweeningManager;

#if DEBUG
        [Inject]
        private readonly SiraLog _log;
#endif

        [UIComponent("StartingInTime")]
        private readonly TextMeshProUGUI TimeoutText;

        [UIComponent("TitleText")]
        private readonly TextMeshProUGUI TitleText;

        [UIComponent("NameText")]
        private readonly TextMeshProUGUI NameText;

        [UIComponent("PauseButton")]
        private readonly Button PauseButton;

        [UIComponent("ResumeButton")]
        private readonly Button ResumeButton;

        [UIComponent("RecreateButton")]
        private readonly Button RecreateButton;

        public string Timeout
        {
            get => TimeoutText.text;
            set => TimeoutText.text = value;
        }

        public bool Visible
        {
            get => _floatingScreen.gameObject.activeInHierarchy;
            set
            {
                _floatingScreen.SetRootViewController(value ? this : null, value ? AnimationType.In : AnimationType.Out);
            }
        }

        public bool HandleVisible
        {
            get => _floatingScreen.handle.gameObject.activeInHierarchy;
            set
            {
                _floatingScreen.handle.gameObject.SetActive(value);
            }
        }

        public bool IsPaused { get; private set; }

        public void Initialize()
        {
            _replayManager.FloatingScreen = this;
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(100f, 50f), true, new Vector3(0f, 2.8f, 2.5f), Quaternion.Euler(new Vector3(320f, 0f, 0f)));
            _floatingScreen.gameObject.SetActive(true);
            _floatingScreen.handle.SetActive(false);
            Visible = false;

            didActivate = (x, y, z) =>
            {
                TitleText.text = $"AwayPlayer v{Plugin.VersionString}";
                NameText.text = $"AwayPlayer v{Plugin.VersionString} by Arimodu";
                if (_config.DisableDeveloperNameText) NameText.gameObject.SetActive(false);
#if DEBUG
                //RecreateButton.gameObject.SetActive(true); 
#endif
            };
        }

        [UIAction("ExitAFKMode")]
        private void ExitAFKMode()
        {
            _replayManager.Enabled = false;
            Visible = false;
        }

        [UIAction("SkipCurrent")]
        private void SkipCurrent()
        {
            if (IsPaused)
            {
                _unityMainThreadDispatcher.ResumeTask(_replayManager.StartJobID);
                ResumeButton.gameObject.SetActive(false);
                PauseButton.gameObject.SetActive(true);
                IsPaused = false;
            }
            _replayManager.SkipCurrentSelection();
        }

        [UIAction("PauseCountdown")]
        private void PauseCountdown()
        {
            IsPaused = true;
            _unityMainThreadDispatcher.PauseTask(_replayManager.StartJobID);
            PauseButton.gameObject.SetActive(false);
            ResumeButton.gameObject.SetActive(true);
        }

        [UIAction("ResumeCountdown")]
        private void ResumeCountdown()
        {
            IsPaused = false;
            _unityMainThreadDispatcher.ResumeTask(_replayManager.StartJobID);
            ResumeButton.gameObject.SetActive(false);
            PauseButton.gameObject.SetActive(true);
        }

        [UIAction("RecreateThis")]
        private void RecreateThis()
        {
#if DEBUG
            try
            {
                var path = "./UserData/AwayPlayerScreenCoordinatesDebugFile.txt";
                if (!File.Exists(path))
                {
                    string x = "70,50,-2,3.5,2,300,0,0";
                    File.WriteAllText(path, x);
                }

                var text = File.ReadAllText(path);
                var parts = text.Split(',');
                var width = float.Parse(parts[0]);
                var height = float.Parse(parts[1]);
                var posX = float.Parse(parts[2]);
                var posY = float.Parse(parts[3]);
                var posZ = float.Parse(parts[4]);
                var angX = float.Parse(parts[5]);
                var angY = float.Parse(parts[6]);
                var angZ = float.Parse(parts[7]);
                _floatingScreen = null;

                _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(width, height), false, new Vector3(posX, posY, posZ), Quaternion.Euler(new Vector3(angX, angY, angZ)));
                _floatingScreen.gameObject.SetActive(true);
                _floatingScreen.SetRootViewController(this,AnimationType.In);
            }
            catch (Exception e)
            {
                _log.Error(e);
            } 
#endif
        }
    }
}
