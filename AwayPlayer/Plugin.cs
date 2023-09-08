using AwayPlayer.Installers;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace AwayPlayer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        internal static IPALogger Logger { get; set; }

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenject)
        {
            Logger = logger;
            zenject.UseLogger(logger);
            zenject.UseMetadataBinder<Plugin>();
            zenject.UseHttpService(SiraUtil.Web.HttpServiceType.UnityWebRequests);

            zenject.Install<APMenuInstaller>(Location.Menu, config.Generated<APConfig>());
        }

        [OnStart]
        public void OnStart()
        {
            var harmony = new Harmony("Arimodu.AwayPlayer.Whatever.Whatever");
            harmony.PatchAll(Assembly);
        }
    }
}
