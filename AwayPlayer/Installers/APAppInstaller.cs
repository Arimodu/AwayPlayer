using AwayPlayer.Managers;
using Zenject;

namespace AwayPlayer.Installers
{
    internal class APAppInstaller : Installer
    {
        private APConfig Config;
        public APAppInstaller(APConfig config)
        {
            Config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(Config);
            Container.BindInterfacesAndSelfTo<UnityMainThreadDispatcher>().AsSingle();
        }
    }
}
