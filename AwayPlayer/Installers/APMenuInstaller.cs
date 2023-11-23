using AwayPlayer.Managers;
using AwayPlayer.Models;
using AwayPlayer.UI;
using AwayPlayer.Utils;
using Zenject;

namespace AwayPlayer.Installers
{
    internal class APMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CacheManager<Score>>().AsSingle();
            Container.BindInterfacesAndSelfTo<APIWrapper>().AsSingle();
            Container.BindInterfacesAndSelfTo<ScoreListManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReplayManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<APMenuFloatingScreen>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<APMainMenu>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<APSettingsMainViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<APSettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<ArgReplayStarter>().AsSingle();
        }
    }
}
