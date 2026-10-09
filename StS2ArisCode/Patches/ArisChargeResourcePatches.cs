using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch]
public static class ArisChargeResourcePatches
{
    [HarmonyPatch(typeof(PlayerCombatState), nameof(PlayerCombatState.HasEnoughResourcesFor))]
    [HarmonyPrefix]
    public static bool HasEnoughResourcesForPrefix(PlayerCombatState __instance, CardModel card, ref UnplayableReason reason, ref bool __result)
    {
        if (card.Owner?.PlayerCombatState == null || ArisCharge.Get(card.Owner) <= 0)
        {
            return true;
        }

        __result = ArisCharge.HasEnoughResourcesFor(__instance, card.Owner, card, out reason);
        return false;
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.SpendResources))]
    [HarmonyPrefix]
    public static bool SpendResourcesPrefix(CardModel __instance, ref Task<(int, int)> __result)
    {
        ArisCharge.ClearSpent(__instance);

        if (__instance.Owner?.PlayerCombatState == null)
        {
            ArisCharge.SetEnergyWasEmptyBeforeSpend(__instance, false);
            return true;
        }

        ArisCharge.SetEnergyWasEmptyBeforeSpend(__instance, __instance.Owner.PlayerCombatState.Energy <= 0);
        if (ArisCharge.Get(__instance.Owner) <= 0)
        {
            return true;
        }

        __result = ArisCharge.SpendResources(__instance);
        return false;
    }

    [HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
    [HarmonyPrefix]
    public static void OnPlayWrapperPrefix(CardModel __instance, bool isAutoPlay, ref ResourceInfo resources)
    {
        if (isAutoPlay)
        {
            return;
        }

        // Charge pays the card's cost without counting as energy actually spent.
        resources = resources with { EnergyValue = resources.EnergyValue + ArisCharge.GetSpent(__instance) };
    }
}
