using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Cards;
using ArisCharacter = StS2Aris.StS2ArisCode.Character.StS2Aris;

namespace StS2Aris.StS2ArisCode.Mechanics;

public static class ArisQuestProgress
{
    private static readonly SavedSpireField<Player, int> CompletedQuestCount = new(() => 0, "ArisCompletedQuestCount");
    private static readonly SavedSpireField<Player, string> CompletedQuestTypes = new(() => "", "ArisCompletedQuestTypes");

    private const char QuestTypeSeparator = '|';

    public static int CountCompletedQuests(Player? player)
    {
        if (player == null)
        {
            return 0;
        }

        int recordedCount = player.RunState.MapPointHistory
            .SelectMany(static act => act)
            .SelectMany(static entry => entry.PlayerStats)
            .Where(entry => entry.PlayerId == player.NetId)
            .Sum(static entry => entry.CompletedQuests.Count);

        return Math.Max(CompletedQuestCount.Get(player), recordedCount);
    }

    public static int CountCompletedQuestTypes(Player? player)
    {
        return player == null ? 0 : GetCompletedQuestTypes(player).Count;
    }

    public static bool MarkCompleted(CardModel questCard)
    {
        var player = questCard.Owner;
        if (player?.Character is not ArisCharacter || !IsTrackedQuest(questCard))
        {
            return false;
        }

        CompletedQuestCount.Set(player, CompletedQuestCount.Get(player) + 1);
        return MarkCompletedQuestType(player, questCard.Id.Entry);
    }

    public static void MarkCompleted(Player player, int amount)
    {
        if (player.Character is not ArisCharacter || amount <= 0)
        {
            return;
        }

        CompletedQuestCount.Set(player, CompletedQuestCount.Get(player) + amount);
    }

    public static bool MarkCompletedQuestType(Player player, string questTypeId)
    {
        if (player.Character is not ArisCharacter || string.IsNullOrWhiteSpace(questTypeId))
        {
            return false;
        }

        var questTypes = GetCompletedQuestTypes(player);
        if (!questTypes.Add(questTypeId))
        {
            return false;
        }

        CompletedQuestTypes.Set(player, string.Join(QuestTypeSeparator, questTypes.Order(StringComparer.Ordinal)));
        return true;
    }

    private static HashSet<string> GetCompletedQuestTypes(Player player)
    {
        return (CompletedQuestTypes.Get(player) ?? "")
            .Split(QuestTypeSeparator, StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static bool IsTrackedQuest(CardModel questCard)
    {
        return questCard is StS2ArisCard { IsArisQuest: true } || questCard.Type == CardType.Quest;
    }
}
