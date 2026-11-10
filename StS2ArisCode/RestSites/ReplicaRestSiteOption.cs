using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Extensions;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.RestSites;

public sealed class ReplicaRestSiteOption(Player owner) : CustomRestSiteOption(owner)
{
    private static readonly string IconAssetPath = "option_replica.png".CharacterUiPath();

    public override string OptionId => "STS2ARIS_REPLICA";

    public override string CustomIconPath => IconAssetPath;

    public override IEnumerable<string> AssetPaths => [IconAssetPath];

    public override bool IsEnabled => GetItemCopyBugs(Owner).Any() && HasValidTarget(Owner);

    public override LocString Description => new("rest_site_ui", IsEnabled
        ? "OPTION_STS2ARIS_REPLICA.description"
        : "OPTION_STS2ARIS_REPLICA.descriptionDisabled");

    public override async Task<bool> OnSelect()
    {
        if (!IsEnabled)
        {
            return false;
        }

        var itemCopyBugs = GetItemCopyBugs(Owner).ToList();
        var rewardTargets = GetRewardTargets(Owner).ToList();
        if (itemCopyBugs.Count == 0 || rewardTargets.Count == 0)
        {
            return false;
        }

        var completedCopies = new List<ItemCopyBug>();
        foreach (var itemCopyBug in itemCopyBugs)
        {
            if (itemCopyBug.QuestRemaining <= 0)
            {
                itemCopyBug.QuestRemaining = itemCopyBug.QuestGoal;
            }

            itemCopyBug.QuestRemaining = int.Max(0, itemCopyBug.QuestRemaining - 1);
            if (itemCopyBug.QuestRemaining == 0)
            {
                completedCopies.Add(itemCopyBug);
            }
        }

        if (completedCopies.Count == 0)
        {
            return true;
        }

        var rewardUpgradeFlags = completedCopies.Select(static card => card.IsUpgraded).ToList();
        foreach (var completedCopy in completedCopies)
        {
            await CardPileCmd.RemoveFromDeck(completedCopy, showPreview: false);
        }

        var addedResults = new List<CardPileAddResult>();
        foreach (var forceUpgrade in rewardUpgradeFlags)
        {
            foreach (var rewardTarget in rewardTargets)
            {
                ArisQuestProgress.MarkCompleted(rewardTarget);
                await ArisQuestProgress.NotifyCompleted(Owner, rewardTarget);

                var addedResult = await ArisQuestUtils.ApplyReplicaRewardFor(rewardTarget, forceUpgrade);
                if (addedResult.HasValue)
                {
                    addedResults.Add(addedResult.Value);
                }
            }
        }

        if (addedResults.Count > 0)
        {
            CardCmd.PreviewCardPileAdd(addedResults, 2f);
        }
        return true;
    }

    public static bool HasValidTarget(Player player)
    {
        return GetRewardTargets(player).Any();
    }

    private static IEnumerable<ItemCopyBug> GetItemCopyBugs(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.OfType<ItemCopyBug>();
    }

    private static IEnumerable<CardModel> GetRewardTargets(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.Where(IsValidTarget);
    }

    private static bool IsValidTarget(CardModel card)
    {
        return card is not ItemCopyBug && ArisQuestUtils.HasSelectableReplicaReward(card);
    }
}
