using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Cards;

namespace StS2Aris.StS2ArisCode.Patches;

[HarmonyPatch]
public static class CompetitiveQuestEnchantmentPatch
{
    public static IEnumerable<MethodBase> TargetMethods()
    {
        return typeof(EnchantmentModel).Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(EnchantmentModel)))
            .Select(type => type.GetMethod(nameof(EnchantmentModel.EnchantDamageAdditive),
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly,
                null, [typeof(decimal), typeof(ValueProp)], null))
            .OfType<MethodInfo>()
            .Where(method => !method.IsAbstract);
    }

    public static void Postfix(EnchantmentModel __instance, ref decimal __result)
    {
        if (__result > 0m && __instance.IsMutable && __instance.HasCard
            && __instance.Card is CompetitiveQuest card)
        {
            __result = card.ScaleEnchantmentDamage(__result);
        }
    }
}
