using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace AwayPlayer.UI
{
    [ViewDefinition("AwayPlayer.UI.BSML.SettingsMainView.bsml")]
    [HotReload(RelativePathToLayout = @"./BSML/SettingsMainView.bsml")]
    internal class APSettingsMainViewController : BSMLAutomaticViewController, IInitializable
    {
        public void Initialize()
        {
            
        }
    }
}
