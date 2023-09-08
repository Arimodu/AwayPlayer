using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.FloatingScreen;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;
using UnityEngine;
using Zenject;

namespace AwayPlayer.UI
{
    [ViewDefinition("AwayPlayer.UI.BSML.MenuFloatingScreen.bsml")]
    [HotReload(RelativePathToLayout = @"BSML\MenuFloatingScreen.bsml")]
    internal class MenuFloatingScreen : BSMLAutomaticViewController, IInitializable
    {
        private FloatingScreen _floatingScreen;

        [Inject]
        private ReplayManager _replayManager;

        [UIComponent("StartingInTime")]
        private readonly TextMeshProUGUI TimeoutText;
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

        public void Initialize()
        {
            _replayManager.FloatingScreen = this;
            _floatingScreen = FloatingScreen.CreateFloatingScreen(new Vector2(70f, 30f), false, new Vector3(0f, 3.5f, 4f), Quaternion.Euler(new Vector3(300f, 0f, 0f)));
            _floatingScreen.gameObject.SetActive(true);
            Visible = false;
        }

        [UIAction("ExitAFKMode")]
        private void ExitAFKMode()
        {
            _replayManager.Enabled = false;
            Visible = false;
        }
    }
}
