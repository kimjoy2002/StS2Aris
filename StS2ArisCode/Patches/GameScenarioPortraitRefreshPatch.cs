using HarmonyLib;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch(typeof(GameScenario), nameof(GameScenario.RefreshGeneratedValues))]
internal static class GameScenarioPortraitRefreshPatch
{
    private static void Postfix(GameScenario __instance)
    {
        CardPortraitCompat.Refresh(__instance);
    }
}
