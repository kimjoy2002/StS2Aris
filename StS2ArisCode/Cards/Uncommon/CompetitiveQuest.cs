using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
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
public class CompetitiveQuest() : StS2ArisCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IArisQuestProgressCard, IArisReplicaReward
{
    private int _damageDoublings;
    private decimal _appliedBaseDamage = 20m;

    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override bool IsArisQuest => true;
    public int QuestProgressCurrent => 0;
    public int QuestProgressGoal => 1;

    [SavedProperty]
    public int DamageDoublings
    {
        get => _damageDoublings;
        set
        {
            AssertMutable();
            _damageDoublings = Math.Max(0, value);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(20m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompat(this, play)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Targeting(play.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        ApplyDamageScaling();
        DynamicVars.Damage.UpgradeValueBy(0m);
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        ApplyDamageScaling();
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        _appliedBaseDamage = 20m;
        ApplyDamageScaling();
    }

    internal async Task CompleteRelicVictory()
    {
        DoubleBaseDamage();
        ArisQuestProgress.MarkCompleted(this);
        CardCmd.Preview(this, 0.5f);
        await BingoBoard.AdvanceBoardsForCompletedQuest(Owner, this);
    }

    public Task<CardPileAddResult?> ApplyReplicaReward(bool forceUpgrade)
    {
        DoubleBaseDamage();
        CardCmd.Preview(this, 1.5f);
        return Task.FromResult<CardPileAddResult?>(null);
    }

    private void DoubleBaseDamage()
    {
        if (DamageDoublings < int.MaxValue)
        {
            DamageDoublings++;
        }
        ApplyDamageScaling();
    }

    private void ApplyDamageScaling()
    {
        const decimal maxDamage = 999999999m;
        decimal damage = IsUpgraded ? 28m : 20m;
        for (var i = 0; i < DamageDoublings && damage < maxDamage; i++)
        {
            damage = Math.Min(maxDamage, damage * 2m);
        }

        DynamicVars.Damage.BaseValue += damage - _appliedBaseDamage;
        _appliedBaseDamage = damage;
    }

    internal decimal ScaleEnchantmentDamage(decimal damage)
    {
        if (damage <= 0m)
        {
            return damage;
        }

        const decimal maxDamage = 999999999m;
        for (var i = 0; i < DamageDoublings && damage < maxDamage; i++)
        {
            damage = Math.Min(maxDamage, damage * 2m);
        }

        return damage;
    }
}
