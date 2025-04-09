using AwayPlayer.Installers;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using System;
using System.Reflection;
using IPALogger = IPA.Logging.Logger;

namespace AwayPlayer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Logger { get; set; }
        internal static Harmony Harmony { get; private set; }
        internal static Assembly Assembly { get; } = Assembly.GetExecutingAssembly();
        internal static Version Version { get; } = new Version(0, 1, 4);

        public static string VersionString => Version.ToString();

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenject)
        {
            Logger = logger;
            zenject.UseLogger(logger);
            zenject.UseMetadataBinder<Plugin>();
            zenject.UseHttpService(SiraUtil.Web.HttpServiceType.UnityWebRequests);

            zenject.Install<APAppInstaller>(Location.App, config.Generated<APConfig>());
            zenject.Install<APMenuInstaller>(Location.Menu);
        }

        [OnStart]
        public void OnStart()
        {
            //Harmony = new Harmony("Arimodu.AwayPlayer");
            //Harmony.PatchAll(Assembly); // Will patch on first focus / defocus cycle
        }

        //[OnExit]
        //public void OnExit() => Harmony.

        /* TODO:
         * - Handle
         * - Floating button
         * - Player settings editor
         * - Filter by FC
         * - Instant load option
         * - Set timeout time option
         * - Multifilter (right settings, middle filters, left single filter, bottom totals)
         * - In game controller
         * - Queue view
         * - Picks / Bans
         * - Small menu integration
         * 
        */
    }
}
