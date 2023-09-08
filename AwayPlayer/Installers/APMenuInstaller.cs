using AwayPlayer.UI;
using Zenject;

namespace AwayPlayer.Installers
{
    internal class APMenuInstaller : Installer
    {
        private APConfig Config;
        public APMenuInstaller(APConfig config)
        {
            Config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(Config);
            Container.BindInterfacesAndSelfTo<UnityMainThreadDispatcher>().AsSingle();
            Container.BindInterfacesAndSelfTo<APIWrapper>().AsSingle();
            Container.BindInterfacesAndSelfTo<ReplayManager>().AsSingle();
            Container.BindInterfacesAndSelfTo<MenuFloatingScreen>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<MainMenu>().AsSingle();
        }
    }
}
