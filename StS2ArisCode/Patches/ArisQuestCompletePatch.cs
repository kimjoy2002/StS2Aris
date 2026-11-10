using System.Runtime.CompilerServices;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Random;
using StS2Aris.StS2ArisCode.Mechanics;
using ArisCharacter = StS2Aris.StS2ArisCode.Character.StS2Aris;

namespace StS2Aris.StS2ArisCode.Patches;

public static class ArisQuestCompletePatch
{
    private static readonly ConditionalWeakTable<CardModel, PendingQuestCompletion> PendingQuestCompletions = new();

    private sealed class PendingQuestCompletion;

    [HarmonyPatch(typeof(PlayerCmd), nameof(PlayerCmd.CompleteQuest))]
    public static class PlayerCmdCompleteQuestPatch
    {
        public static void Postfix(CardModel questCard)
        {
            ArisQuestProgress.MarkCompleted(questCard);

            if (questCard.Owner.Character is ArisCharacter && questCard.Type == CardType.Quest)
            {
                PendingQuestCompletions.GetValue(questCard, static _ => new PendingQuestCompletion());
            }
        }
    }

    [HarmonyPatch(
        typeof(CardCmd),
        nameof(CardCmd.Transform),
        [typeof(IEnumerable<CardTransformation>), typeof(Rng), typeof(CardPreviewStyle)])]
    public static class CardCmdTransformPatch
    {
        public static void Prefix(
            ref IEnumerable<CardTransformation> transformations,
            out List<CardModel> __state)
        {
            var materialized = transformations.ToArray();
            transformations = materialized;
            __state = materialized
                .Select(static transformation => transformation.Original)
                .Where(IsPendingQuestCompletion)
                .Distinct()
                .ToList();
        }

        public static void Postfix(
            List<CardModel> __state,
            ref Task<IEnumerable<CardPileAddResult>> __result)
        {
            if (__state.Count > 0)
            {
                __result = CompletePendingQuestsAfter(__result, __state);
            }
        }
    }

    [HarmonyPatch(
        typeof(CardPileCmd),
        nameof(CardPileCmd.RemoveFromDeck),
        [typeof(IReadOnlyList<CardModel>), typeof(bool)])]
    public static class CardPileCmdRemoveFromDeckPatch
    {
        public static void Prefix(IReadOnlyList<CardModel> cards, out List<CardModel> __state)
        {
            __state = cards.Where(IsPendingQuestCompletion).Distinct().ToList();
        }

        public static void Postfix(List<CardModel> __state, ref Task __result)
        {
            if (__state.Count > 0)
            {
                __result = CompletePendingQuestsAfter(__result, __state);
            }
        }
    }

    [HarmonyPatch(typeof(Byrdpip), nameof(Byrdpip.AfterObtained))]
    public static class ByrdpipAfterObtainedPatch
    {
        public static void Prefix(Byrdpip __instance, out List<CardModel> __state)
        {
            var player = __instance.Owner;
            if (player.Character is not ArisCharacter)
            {
                __state = [];
                return;
            }

            __state = PileType.Deck.GetPile(player).Cards.Where(static card => card is ByrdonisEgg).ToList();
            if (CombatManager.Instance.IsInProgress && player.PlayerCombatState != null)
            {
                __state.AddRange(player.PlayerCombatState.AllCards.Where(static card => card is ByrdonisEgg));
            }
        }

        public static void Postfix(Byrdpip __instance, List<CardModel> __state, ref Task __result)
        {
            if (__state.Count > 0)
            {
                __result = CompleteByrdpipQuestAfter(__result, __instance.Owner, __state);
            }
        }
    }

    private static bool IsPendingQuestCompletion(CardModel card)
    {
        return PendingQuestCompletions.TryGetValue(card, out _);
    }

    private static async Task<IEnumerable<CardPileAddResult>> CompletePendingQuestsAfter(
        Task<IEnumerable<CardPileAddResult>> originalTask,
        IReadOnlyList<CardModel> questCards)
    {
        var result = await originalTask;
        await CompletePendingQuests(questCards);
        return result;
    }

    private static async Task CompletePendingQuestsAfter(Task originalTask, IReadOnlyList<CardModel> questCards)
    {
        await originalTask;
        await CompletePendingQuests(questCards);
    }

    private static async Task CompletePendingQuests(IReadOnlyList<CardModel> questCards)
    {
        foreach (var questCard in questCards)
        {
            if (PendingQuestCompletions.Remove(questCard))
            {
                await ArisQuestProgress.NotifyCompleted(questCard.Owner, questCard);
            }
        }
    }

    private static async Task CompleteByrdpipQuestAfter(
        Task originalTask,
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        IReadOnlyList<CardModel> eggCards)
    {
        await originalTask;

        var eggCard = eggCards[0];
        ArisQuestProgress.MarkCompletedQuestType(player, eggCard.Id.Entry);
        ArisQuestProgress.MarkCompleted(player, eggCards.Count);
        await ArisQuestProgress.NotifyCompleted(player, eggCard, eggCards.Count);
    }
}
