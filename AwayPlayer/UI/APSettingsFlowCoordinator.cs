/*using AwayPlayer.Managers;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.MenuButtons;
using HMUI;
using SiraUtil.Logging;
using Zenject;

#pragma warning disable CS0649 // Value is never assigend to - We have zenject
namespace AwayPlayer.UI
{
    internal class APSettingsFlowCoordinator : FlowCoordinator, IInitializable
    {
        [Inject]
        private readonly APMainMenu MainMenuFlowCoordinator;

        [Inject]
        private readonly ScoreListManager ScoreListManager;

        [Inject] private readonly SiraLog Logger;

        private MenuButton APButton;

        protected override void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
        {
            if (firstActivation)
            {
                SetTitle($"AwayPlayer v{Plugin.VersionString}");
                ProvideInitialViewControllers(MainMenuFlowCoordinator);
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
            MenuButtons.Instance.RegisterButton(APButton);
            ScoreListManager.OnScorelistUpdated += () => 
            { 
                APButton.Interactable = ScoreListManager.IsReady; 
                MenuButtons.Instance.RegisterButton(APButton); 
                Logger.Notice($"Updated menu button interactivity to {ScoreListManager.IsReady}"); 
            };
        }
        private void OnMenuButtonPressed()
        {
            if (MainMenuFlowCoordinator == null) return;
            BeatSaberUI.MainFlowCoordinator.PresentFlowCoordinatorOrAskForTutorial(this);
        }
    }
}
*/