using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class CursedWeapon() : ArisQuestCard<BlessedSword>(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    private bool _rewardDuringRemoval;

    public override int QuestGoal => 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward),
        HoverTipFactory.FromKeyword(ArisKeywords.Equipment),
        HoverTipFactory.FromKeyword(ArisKeywords.ClassChange),
        HoverTipFactory.FromCard<BlessedSword>(IsUpgraded)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(11, ValueProp.Move)];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play)
            .Targeting(play.Target).Execute(choiceContext);
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (_rewardDuringRemoval || card.Owner != Owner || card.Pile?.Type != PileType.Deck ||
            (card != this && card.Type != CardType.Curse))
        {
            return;
        }

        _rewardDuringRemoval = card == this;
        try
        {
            await CompleteQuest();
        }
        finally
        {
            _rewardDuringRemoval = false;
        }
    }

    protected override async Task ApplyQuestReward()
    {
        if (_rewardDuringRemoval)
        {
            CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(CreateRewardCard(), PileType.Deck));
            return;
        }

        await base.ApplyQuestReward();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
