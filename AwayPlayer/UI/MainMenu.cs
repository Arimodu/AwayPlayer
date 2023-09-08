using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.GameplaySetup;
using System.Threading.Tasks;
using Zenject;

#pragma warning disable IDE0051 // Remove unused private members
namespace AwayPlayer.UI
{
    internal class MainMenu : IInitializable
    {
        [Inject]
        private readonly ReplayManager _replayManager;

        [Inject]
        private readonly UnityMainThreadDispatcher _unityMainThreadDispatcher;

        [Inject]
        private readonly MenuFloatingScreen _menuFloatingScreen;

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("AwayPlayer", "AwayPlayer.UI.BSML.MainMenu.bsml", this);
        }

        [UIAction("enable_button")]
        private void EnableButton()
        {
            _replayManager.Enabled = true;
            _replayManager.Setup();
            Task.Run(() => _unityMainThreadDispatcher.EnqueueWithDelay(_replayManager.StartReplayAsync, 10000, (remaining) => _menuFloatingScreen.Timeout = remaining.ToString()));
            _menuFloatingScreen.Visible = true;
        }
    }
}
