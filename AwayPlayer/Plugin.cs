using AwayPlayer.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using SiraUtil.Zenject;
using System;
using IPALogger = IPA.Logging.Logger;

namespace AwayPlayer
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    [NoEnableDisable]
    public class Plugin
    {
        internal static IPALogger Logger { get; set; }
        internal static Version Version = new Version(0,0,1);

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

        /* TODO:
         * - Caching???
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
