using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.CardModels;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Powers;
using StS2Aris.StS2ArisCode.Utils;
using StS2Aris.StS2ArisCode.Vfx;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(TokenCardPool))]
public class LuminousNovaShot() : StS2ArisCard(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy), IOverload
{
    private static readonly Dictionary<Player, decimal> CombatDamageBonuses = new();
    private decimal _appliedCombatDamageBonus;

    protected override IEnumerable<string> ExtraRunAssetPaths =>
    [
        NEnergyProjectionVfx.OrbAtlasPath,
        NEnergyProjectionVfx.ImpactScenePath
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        ArisHoverTips.ChargePower(),
        HoverTipFactory.FromKeyword(ArisKeywords.Overload)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7m, ValueProp.Move),
        new PowerVar<ChargePower>(1m),
        new DynamicVar("Increase", 5m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }
        var player = play.GetPlayer();

        var attack = DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCardCompat(this, play)
            .Targeting(play.Target);
        if (ArisEquipment.IsSuperNovaEquipped(player))
        {
            attack.WithAttackerAnim("Attack2", player.Character.AttackAnimDelay);
        }

        await attack
            .BeforeDamage(async () =>
            {
                StS2ArisMain.PlayAttackSfx("Aris_laser.mp3".SfxPath());
                NEnergyProjectionVfx? projectile = NEnergyProjectionVfx.Create(player.Creature, play.Target);
                if (projectile != null)
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(projectile);

                await Cmd.Wait(NEnergyProjectionVfx.TravelDuration);
            })
            .Execute(choiceContext);
        await PowerCmd.Apply<ChargePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ChargePower"].BaseValue,
            Owner.Creature,
            this);
    }

    public Task OnOverload(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = Owner.PlayerCombatState;
        if (combatState == null)
        {
            return Task.CompletedTask;
        }

        var increase = DynamicVars["Increase"].BaseValue;
        CombatDamageBonuses[Owner] = CombatDamageBonuses.GetValueOrDefault(Owner) + increase;
        foreach (var shot in combatState.AllCards.OfType<LuminousNovaShot>())
        {
            shot.AddCombatDamageBonus(increase);
        }

        return Task.CompletedTask;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card == this && CombatDamageBonuses.TryGetValue(Owner, out var totalBonus))
        {
            AddCombatDamageBonus(totalBonus - _appliedCombatDamageBonus);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Increase"].UpgradeValueBy(1m);
    }

    protected override void AfterDowngraded()
    {
        base.AfterDowngraded();
        DynamicVars.Damage.BaseValue += _appliedCombatDamageBonus;
    }

    public static void ResetCombatDamageBonuses()
    {
        CombatDamageBonuses.Clear();
    }

    private void AddCombatDamageBonus(decimal amount)
    {
        if (amount <= 0m)
        {
            return;
        }

        DynamicVars.Damage.BaseValue += amount;
        _appliedCombatDamageBonus += amount;
    }
}
