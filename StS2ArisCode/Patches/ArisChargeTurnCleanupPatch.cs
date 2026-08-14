using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch(typeof(CombatManager), "StartCombatInternal")]
public static class ArisCombatStartCleanupPatch
{
    [HarmonyPrefix]
    public static void ResetOverloadCount()
    {
        ArisCharge.ResetOverloadCount();
        GameScenario.ResetChronicleCount();
        HeroSword.ResetCombatCounters();
        LuminousNovaShot.ResetCombatDamageBonuses();
    }
}

[HarmonyPatch]
public static class ArisChargeTurnCleanupPatch
{
    private const string MethodName = "EndPlayerTurnPhaseTwoInternal";
    private const string CombatTurnStateTypeName = "MegaCrit.Sts2.Core.Combat.CombatTurnState";

    public static MethodBase TargetMethod()
    {
        var methods = AccessTools.GetDeclaredMethods(typeof(CombatManager))
            .Where(method => method.Name == MethodName)
            .ToList();

        return methods.FirstOrDefault(method =>
                   method.GetParameters() is [{ ParameterType.FullName: CombatTurnStateTypeName }])
               ?? methods.Single(method => method.GetParameters().Length == 0);
    }

    [HarmonyPrefix]
    public static void ClearChargeAtTurnEnd(CombatManager __instance)
    {
        var state = __instance.DebugOnlyGetState();
        if (state?.CurrentSide != CombatSide.Player)
        {
            return;
        }

        foreach (var player in state.Players)
        {
            if (player.Creature.GetPower<AuxiliaryPower>() != null)
            {
                continue;
            }

            if (player.Creature.GetPower<ChargeRetentionPower>() is { } retentionPower)
            {
                retentionPower.RemoveInternal();
                continue;
            }

            player.Creature.GetPower<ChargePower>()?.RemoveInternal();
        }
    }
}
