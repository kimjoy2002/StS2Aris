using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class LibrarianStrike() : StS2ArisCard(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), IArisQuestProgressCard, IArisReplicaReward
{
    private const int QuestGoalValue = 3;
    private int _permanentDamageBonus;
    private int _appliedPermanentDamageBonus;
    private int _questRemaining;

    public override bool IsArisQuest => true;
    public int QuestProgressCurrent => int.Clamp(QuestGoalValue - QuestRemaining, 0, QuestProgressGoal);
    public int QuestProgressGoal => QuestGoalValue;

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    [SavedProperty]
    public int PermanentDamageBonus
    {
        get => _permanentDamageBonus;
        set
        {
            AssertMutable();
            _permanentDamageBonus = value;
        }
    }

    [SavedProperty]
    public int QuestRemaining
    {
        get => _questRemaining;
        set
        {
            AssertMutable();
            _questRemaining = value;
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        new DynamicVar("PermanentDamage", 5m)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompat(this, play)
            .Targeting(play.Target)
            .Execute(choiceContext);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (Pile?.Type != PileType.Deck
            || card.Pile?.Type != PileType.Deck
            || Owner != card.Owner
            || card == this
            || card.Id == Id)
        {
            return;
        }

        await AdvanceQuest();
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        EnsureQuestInitialized();
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        EnsureQuestInitialized();
        ApplyPermanentDamageBonus();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["PermanentDamage"].UpgradeValueBy(2m);
    }

    public Task<CardPileAddResult?> ApplyReplicaReward(bool forceUpgrade)
    {
        var damageBonus = DynamicVars["PermanentDamage"].IntValue;
        if (forceUpgrade && !IsUpgraded)
        {
            damageBonus++;
        }

        PermanentDamageBonus += damageBonus;
        ApplyPermanentDamageBonus();
        CardCmd.Preview(this, 1.5f);
        return Task.FromResult<CardPileAddResult?>(null);
    }

    private async Task AdvanceQuest()
    {
        EnsureQuestInitialized();
        QuestRemaining = int.Max(0, QuestRemaining - 1);
        if (QuestRemaining > 0)
        {
            return;
        }

        PermanentDamageBonus += DynamicVars["PermanentDamage"].IntValue;
        ApplyPermanentDamageBonus();
        ArisQuestProgress.MarkCompleted(this);
        QuestRemaining = QuestGoalValue;
        CardCmd.Preview(this, 0.5f);
        await ArisQuestProgress.NotifyCompleted(Owner, this);
    }

    private void EnsureQuestInitialized()
    {
        if (QuestRemaining <= 0)
        {
            QuestRemaining = QuestGoalValue;
        }
    }

    private void ApplyPermanentDamageBonus()
    {
        var delta = PermanentDamageBonus - _appliedPermanentDamageBonus;
        if (delta == 0)
        {
            return;
        }

        DynamicVars.Damage.BaseValue += delta;
        _appliedPermanentDamageBonus = PermanentDamageBonus;
    }
}
