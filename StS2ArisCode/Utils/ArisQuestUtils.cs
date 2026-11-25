using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Utils;

public static class ArisQuestUtils
{
    private const string DowsingTypeName = "MegaCrit.Sts2.Core.Models.Cards.Dowsing";
    private const string AbundanceTypeName = "MegaCrit.Sts2.Core.Models.Cards.Abundance";

    public static int CountCompletedQuests(MegaCrit.Sts2.Core.Entities.Players.Player? player) => ArisQuestProgress.CountCompletedQuests(player);

    public static async Task<CardPileAddResult?> ApplyReplicaRewardFor(CardModel quest, bool forceUpgrade = false)
    {
        if (quest is IArisReplicaReward arisReward)
        {
            return await arisReward.ApplyReplicaReward(forceUpgrade);
        }

        switch (quest)
        {
            case LanternKey:
                await RelicCmd.Obtain<HistoryCourse>(quest.Owner);
                return null;
            case SpoilsMap:
                await PlayerCmd.GainGold(600m, quest.Owner);
                return null;
            case ByrdonisEgg byrdonisEgg:
                return await CardPileCmd.Add(CreateReward<ByrdSwoop>(byrdonisEgg, forceUpgrade), PileType.Deck);
            case CardModel dowsing when IsCardType(dowsing, DowsingTypeName):
                CardModel? reward = CreateRewardByTypeName(dowsing, AbundanceTypeName, forceUpgrade);
                return reward == null ? null : await CardPileCmd.Add(reward, PileType.Deck);
        }

        return null;
    }

    public static bool HasSelectableReplicaReward(CardModel quest)
    {
        return quest is IArisReplicaReward or LanternKey or SpoilsMap or ByrdonisEgg
               || IsCardType(quest, DowsingTypeName);
    }

    public static void TryCopyEnchantment(CardModel source, CardModel reward)
    {
        var enchantment = source.Enchantment;
        if (enchantment == null || !enchantment.CanEnchant(reward))
        {
            return;
        }

        CardCmd.Enchant((EnchantmentModel)enchantment.ClonePreservingMutability(), reward, enchantment.Amount);
    }

    private static TReward CreateReward<TReward>(CardModel quest, bool forceUpgrade) where TReward : CardModel
    {
        var reward = quest.Owner.RunState.CreateCard<TReward>(quest.Owner);
        if (forceUpgrade || quest.IsUpgraded)
        {
            CardCmdCompat.UpgradeInternal(reward);
            reward.FinalizeUpgradeInternal();
        }

        TryCopyEnchantment(quest, reward);
        return reward;
    }

    private static CardModel? CreateRewardByTypeName(CardModel quest, string rewardTypeName, bool forceUpgrade)
    {
        CardModel? canonicalReward = ModelDb.AllCards.FirstOrDefault(card => IsCardType(card, rewardTypeName));
        if (canonicalReward == null)
        {
            return null;
        }

        CardModel reward = quest.Owner.RunState.CreateCard(canonicalReward, quest.Owner);
        if (forceUpgrade || quest.IsUpgraded)
        {
            CardCmdCompat.UpgradeInternal(reward);
            reward.FinalizeUpgradeInternal();
        }

        TryCopyEnchantment(quest, reward);
        return reward;
    }

    private static bool IsCardType(CardModel card, string fullTypeName)
    {
        return card.GetType().FullName == fullTypeName;
    }
}
