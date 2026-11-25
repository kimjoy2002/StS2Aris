using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class TemporaryWeapon() : ArisQuestCard<TemporaryWeapon>(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public override int QuestGoal => 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12, ValueProp.Move)];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompat(this, play)
            .Targeting(play.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        await CompleteQuestIf(room.RoomType == RoomType.Boss);
    }

    protected override CardModel CreateRewardCard()
    {
        var options = Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(card => card.Type == CardType.Attack && card.Rarity == CardRarity.Uncommon && card.CanBeGeneratedByModifiers)
            .ToList();
        var canonical = options.Count > 0
            ? Owner.RunState.Rng.CombatCardGeneration.NextItem(options) ?? ModelDb.Card<ArisStrike>()
            : ModelDb.Card<ArisStrike>();
        var reward = Owner.RunState.CreateCard(canonical, Owner);
        if (IsUpgraded && reward.IsUpgradable)
        {
            CardCmdCompat.UpgradeInternal(reward);
            reward.FinalizeUpgradeInternal();
        }

        CopyEnchantmentToReward(reward);
        return reward;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
