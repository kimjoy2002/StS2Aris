using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace StS2Aris.StS2ArisCode.Utils;

internal static class CardPortraitCompat
{
    private static readonly MethodInfo? UpdatePortraitMethod =
        AccessTools.DeclaredMethod(typeof(NCard), "UpdatePortrait");

    private static readonly MethodInfo? ReloadMethod =
        AccessTools.DeclaredMethod(typeof(NCard), "Reload");

    public static void Refresh(CardModel card)
    {
        var cardNode = NCard.FindOnTable(card);
        if (cardNode == null)
        {
            return;
        }

        (UpdatePortraitMethod ?? ReloadMethod)?.Invoke(cardNode, null);
    }
}
