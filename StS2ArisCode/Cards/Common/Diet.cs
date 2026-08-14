using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Rewards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Powers;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Cards;
[Pool(typeof(StS2ArisCardPool))]
public class Diet() : ArisQuestCard<NeatCompression>(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    private const string QuestSkipOptionId = "STS2ARIS_DIET_SKIP";
    private static readonly HashSet<Reward> QuestSkippedRewards = [];

    public override int QuestGoal => 3;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward),
        HoverTipFactory.FromCard<NeatCompression>(IsUpgraded),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Magic", 1m)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var amount = DynamicVars["Magic"].IntValue;
        var selected = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, amount)
            {
                Cancelable = true
            },
            null,
            this);

        foreach (var card in selected.ToList())
        {
            await CardCmdCompat.Exhaust(choiceContext, card);
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public override Task AfterRewardTaken(Player player, Reward reward)
    {
        if (player == Owner && reward is CardReward or SpecialCardReward && !QuestSkippedRewards.Contains(reward))
        {
            ResetQuestProgress();
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyCardRewardAlternatives(Player player, CardReward cardReward, List<CardRewardAlternative> alternatives)
    {
        if (player != Owner || Pile?.Type != PileType.Deck || !cardReward.CanSkip || alternatives.Any(static alternative => alternative.OptionId == QuestSkipOptionId))
        {
            return false;
        }

        CardRewardAlternative questSkip = new(
            QuestSkipOptionId,
            () => NotifyCardRewardSkipped(player, cardReward),
            PostAlternateCardRewardAction.EndSelectionAndCompleteReward);

        int rerollIndex = alternatives.FindIndex(static alternative => alternative.OptionId == "REROLL");
        if (rerollIndex >= 0)
        {
            alternatives.Insert(rerollIndex, questSkip);
        }
        else
        {
            alternatives.Add(questSkip);
        }

        return true;
    }

    public static Task NotifyCardRewardSkipped(Player player, Reward reward)
    {
        QuestSkippedRewards.Add(reward);
        return NotifyCardRewardSkipped(player);
    }

    public static async Task NotifyCardRewardSkipped(Player player)
    {
        foreach (Diet quest in PileType.Deck.GetPile(player).Cards.OfType<Diet>().ToList())
        {
            await quest.AdvanceQuest();
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(1m);
    }
}
