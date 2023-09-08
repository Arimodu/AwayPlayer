using BeatLeader.Models;
using BeatLeader.Models.AbstractReplay;
using BeatLeader.Utils;
using HarmonyLib;
using Newtonsoft.Json;

namespace AwayPlayer.HarmonyPatches
{
    //[HarmonyPatch(typeof(AbstractReplayUtils))]
    //[HarmonyPatch("CreateTransitionData")]
    //public static class BLDebugPatch
    //{
    //    static bool Prefix(ReplayLaunchData launchData, PlayerDataModel playerModel, ref StandardLevelScenesTransitionSetupDataSO ___standardLevelScenesTransitionSetupDataSo, ref EnvironmentTypeSO ___normalEnvironmentType, ref StandardLevelScenesTransitionSetupDataSO __result)
    //    {
    //        Plugin.Logger.Info("Start of patched method");
    //        StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupDataSO = ___standardLevelScenesTransitionSetupDataSo;
    //        PlayerData playerData = playerModel.playerData;
    //        bool num = launchData.EnvironmentInfo != null;
    //        OverrideEnvironmentSettings overrideEnvironmentSettings = (num ? new OverrideEnvironmentSettings
    //        {
    //            overrideEnvironments = true
    //        } : playerData.overrideEnvironmentSettings);
    //        if (num)
    //        {
    //            overrideEnvironmentSettings.SetEnvironmentInfoForType(___normalEnvironmentType, launchData.EnvironmentInfo);
    //        }
    //        Plugin.Logger.Info("Past SetEnvironmentInfoForType");
    //        IReplay mainReplay = launchData.MainReplay;
    //        PracticeSettings practiceSettings = (launchData.IsBattleRoyale ? null : launchData.MainReplay.ReplayData.PracticeSettings);
    //        IDifficultyBeatmap difficultyBeatmap = launchData.DifficultyBeatmap;
    //        Plugin.Logger.Info("Before init");
    //        try
    //        {
    //            Plugin.Logger.Info($"ReplayLaunchData: {launchData}");
    //            Plugin.Logger.Info($"OverrideEnvironmentSettings: {JsonConvert.SerializeObject(overrideEnvironmentSettings)}");
    //            Plugin.Logger.Info($"OverrideColorScheme: {JsonConvert.SerializeObject(playerData.colorSchemesSettings.GetOverrideColorScheme())}");
    //            Plugin.Logger.Info($"DifficultyBeatmap: {JsonConvert.SerializeObject(mainReplay.ReplayData.GameplayModifiers)}");
    //            Plugin.Logger.Info($"PlayerSettingsByReplay: {JsonConvert.SerializeObject(playerData.playerSpecificSettings.GetPlayerSettingsByReplay(mainReplay))}");
    //            Plugin.Logger.Info($"PracticeSettings: {JsonConvert.SerializeObject(practiceSettings)}");

    //        }
    //        catch (System.Exception e)
    //        {
    //            Plugin.Logger.Error(e.ToString());
    //        }
    //        standardLevelScenesTransitionSetupDataSO.Init("Solo", difficultyBeatmap, difficultyBeatmap.level, overrideEnvironmentSettings, playerData.colorSchemesSettings.GetOverrideColorScheme(), mainReplay.ReplayData.GameplayModifiers, playerData.playerSpecificSettings.GetPlayerSettingsByReplay(mainReplay), practiceSettings, "Menu");
    //        Plugin.Logger.Info("Past init");
    //        __result = standardLevelScenesTransitionSetupDataSO;
    //        return false;
    //    }
    //}
}
