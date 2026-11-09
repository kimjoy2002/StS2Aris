using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.TreasureRelicPicking;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Cards;

namespace StS2Aris.StS2ArisCode.Patches;

public static class CompetitiveQuestRelicPatch
{
    private static readonly ConditionalWeakTable<Player, PendingVictory> PendingVictories = new();

    public sealed record PendingVictory(ModelId RelicId, CompetitiveQuest[] Cards);

    [HarmonyPatch(typeof(RelicPickingResult), nameof(RelicPickingResult.GenerateRelicFight))]
    public static class RecordWinnerPatch
    {
        public static void Postfix(RelicPickingResult __result)
        {
            if (__result.type != RelicPickingResultType.FoughtOver || __result.player is not { } winner)
            {
                return;
            }

            PendingVictories.Remove(winner);
            var cards = PileType.Deck.GetPile(winner).Cards.OfType<CompetitiveQuest>().ToArray();
            if (cards.Length > 0)
            {
                PendingVictories.Add(winner, new PendingVictory(__result.relic.Id, cards));
            }
        }
    }

    [HarmonyPatch(typeof(RelicCmd), nameof(RelicCmd.Obtain), [typeof(RelicModel), typeof(Player), typeof(int)])]
    public static class CompleteAfterObtainPatch
    {
        public static void Prefix(RelicModel relic, Player player, out PendingVictory? __state)
        {
            __state = null;
            if (PendingVictories.TryGetValue(player, out var victory) && victory.RelicId == relic.Id)
            {
                PendingVictories.Remove(player);
                __state = victory;
            }
        }

        public static void Postfix(Player player, PendingVictory? __state, ref Task<RelicModel> __result)
        {
            if (__state != null)
            {
                __result = CompleteAfterObtain(__result, player, __state);
            }
        }
    }

    private static async Task<RelicModel> CompleteAfterObtain(Task<RelicModel> obtainTask, Player player, PendingVictory victory)
    {
        var relic = await obtainTask;
        foreach (var card in victory.Cards)
        {
            if (!card.HasBeenRemovedFromState && card.Owner == player && card.Pile?.Type == PileType.Deck)
            {
                await card.CompleteRelicVictory();
            }
        }
        return relic;
    }
}
