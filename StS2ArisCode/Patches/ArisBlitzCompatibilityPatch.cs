using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Cards;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch]
public static class ArisBlitzCompatibilityPatch
{
    public static MethodBase? TargetMethod()
    {
        return AccessTools.PropertyGetter(typeof(CardModel), "IsAllowedInBlitz");
    }

    public static bool Prepare()
    {
        return TargetMethod() != null;
    }

    public static void Postfix(CardModel __instance, ref bool __result)
    {
        if (__instance is Grinding || __instance is EmergencyFund)
        {
            __result = false;
        }
    }
}
