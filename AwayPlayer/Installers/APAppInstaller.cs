using AwayPlayer.HarmonyPatches;
using System;
using System.Linq;
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
            var args = Environment.GetCommandLineArgs();

            Container.BindInstance(Config);
            Container.BindInterfacesAndSelfTo<UnityMainThreadDispatcher>().AsSingle();
            if (args.Contains("fpfc") || args.Contains("Fpfc") || args.Contains("FPFC")) Container.BindInterfacesAndSelfTo<DontEatMyCursor>().FromNewComponentOnRoot().AsSingle();
        }
    }
}
