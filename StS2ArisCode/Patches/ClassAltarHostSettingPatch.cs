using HarmonyLib;
using MegaCrit.Sts2.Core.Runs;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch(typeof(RunManager), nameof(RunManager.Launch))]
public static class ClassAltarHostSettingPatch
{
    [HarmonyPrefix]
    public static void SyncHostSetting(RunManager __instance)
    {
        ClassAltarHostSettingSync.InitializeForRun(__instance.NetService);
    }
}
