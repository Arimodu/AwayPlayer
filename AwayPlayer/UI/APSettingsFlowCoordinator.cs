using AwayPlayer.Managers;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using Zenject;

#pragma warning disable CS0649 // Value is never assigend to - We have zenject
namespace AwayPlayer.UI
{
    internal class APSettingsFlowCoordinator : FlowCoordinator, IInitializable
    {
        [Inject]
        private readonly APSettingsMainViewController SettingsMainViewController;

        [Inject]
        private readonly APMainMenu MainMenuFlowCoordinator;

        [Inject]
        private readonly ScoreListManager ScoreListManager;

        private MenuButton APButton;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle($"AwayPlayer v{Plugin.VersionString}");
                ProvideInitialViewControllers(SettingsMainViewController, MainMenuFlowCoordinator);
            }

            showBackButton = true;
        }

        protected override void BackButtonWasPressed(ViewController topViewController)
        {
            MainMenuFlowCoordinator.Reload();
            BeatSaberUI.MainFlowCoordinator.DismissFlowCoordinator(this);
        }

        public void Initialize()
        {
            APButton = new MenuButton("AwayPlayer", "Go get a coffee, Ill entertain your stream for you", OnMenuButtonPressed, ScoreListManager.IsReady);
            MenuButtons.instance.RegisterButton(APButton);
            ScoreListManager.OnScorelistUpdated += ScoreListManager_OnScorelistUpdated;
        }

        private void ScoreListManager_OnScorelistUpdated()
        {
            ScoreListManager.OnScorelistUpdated -= ScoreListManager_OnScorelistUpdated; // Only the first time is needed
            APButton.Interactable = true;
        }

        private void OnMenuButtonPressed()
        {
            if (MainMenuFlowCoordinator == null) return;
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinatorOrAskForTutorial(this);
        }
    }
}
