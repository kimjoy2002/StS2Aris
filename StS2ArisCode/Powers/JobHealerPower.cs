using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;

namespace StS2Aris.StS2ArisCode.Powers;

public sealed class JobHealerPower : ArisJobPower
{
    public const decimal HealAmount = 12m;

    public override string AnimationSuffix => "healer";

    public override async Task OnClassChange(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var equipment = EquipmentCard;
        var player = equipment?.Owner;
        var combatState = player?.Creature.CombatState;
        if (equipment == null || player == null || combatState == null)
        {
            return;
        }

        foreach (var teammate in combatState.Players.Select(p => p.Creature).Where(c => c.IsAlive).ToList())
        {
            await PowerCmd.Apply<ClericStaffStrengthPower>(choiceContext, teammate,
                equipment.DynamicVars["StrengthPower"].BaseValue, player.Creature, equipment);
        }
    }

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (Owner.IsDead || potion.Owner != PlayerOwner)
        {
            return;
        }

        if (potion is FoulPotion)
        {
            var combatState = Owner.CombatState;
            if (combatState == null)
            {
                return;
            }

            var teammates = combatState.Players.Select(player => player.Creature)
                .Where(creature => creature != Owner && creature.IsAlive && creature.Side == Owner.Side)
                .ToList();
            if (teammates.Count > 0)
            {
                Flash();
                foreach (var teammate in teammates)
                {
                    await CreatureCmd.Heal(teammate, HealAmount);
                }
            }
            return;
        }

        if (target?.Player == null || target == Owner || target.IsDead || target.Side != Owner.Side)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(target, HealAmount);
    }

    public override void AddDumbVariablesToPowerDescription(LocString description)
    {
        base.AddDumbVariablesToPowerDescription(description);
        description.Add("Heal", HealAmount);
    }
}
