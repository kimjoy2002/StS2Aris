using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Hooks;
using StS2Aris.StS2ArisCode.Powers;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Mechanics;

public static class ArisEquipment
{
    public static ArisJobPower? GetCurrentJob(Player player)
    {
        return player.Creature.Powers.OfType<ArisJobPower>().FirstOrDefault();
    }

    public static string? GetCurrentAnimationSuffix(Player player)
    {
        return GetCurrentJob(player)?.AnimationSuffix;
    }

    public static bool IsSuperNovaEquipped(Player player)
    {
        return GetCurrentJob(player) is JobAoePower or JobAtrahasisSuperNovaPower;
    }

    public static bool ShouldTriggerClassChange(Player player, ArisJobPower nextJob)
    {
        var currentJob = GetCurrentJob(player);
        return currentJob?.GetType() != nextJob.GetType();
    }

    public static async Task Equip(PlayerChoiceContext choiceContext, CardModel equipmentCard, ArisJobPower nextJob)
    {
        var owner = equipmentCard.Owner;
        if (owner.Creature.IsDead)
        {
            return;
        }

        var previousJob = GetCurrentJob(owner);
        var changed = previousJob == null || previousJob.GetType() != nextJob.GetType();

        if (previousJob != null)
        {
            await previousJob.OnUnequipped(choiceContext, PileType.Discard);

            if (previousJob.EquipmentCard != null)
            {
                await ReturnEquipmentCard(choiceContext, previousJob.EquipmentCard, PileType.Discard);
            }

            await PowerCmd.Remove(previousJob);
        }

        nextJob.EquipmentCard = equipmentCard;
        HoldPlayedEquipmentCard(equipmentCard);
        await PowerCmd.Apply(choiceContext, nextJob, owner.Creature, 1m, owner.Creature, equipmentCard);
        await CreatureCmd.TriggerAnim(owner.Creature, "Idle", 0f);

        if (changed)
        {
            await ArisHook.OnJobChanged(choiceContext, owner, previousJob, nextJob, equipmentCard);
        }
    }

    public static async Task ReturnCurrentJob(PlayerChoiceContext choiceContext, Player player, PileType pileType)
    {
        var currentJob = GetCurrentJob(player);
        if (currentJob == null)
        {
            return;
        }

        await currentJob.OnUnequipped(choiceContext, pileType);
        await ArisHook.OnJobChanged(choiceContext, player, currentJob, null, currentJob.EquipmentCard);
        await PowerCmd.Remove(currentJob);
        if (currentJob.EquipmentCard != null)
        {
            await ReturnEquipmentCard(choiceContext, currentJob.EquipmentCard, pileType);
        }

        await CreatureCmd.TriggerAnim(player.Creature, "Idle", 0f);
    }

    public static async Task TriggerClassChange(PlayerChoiceContext choiceContext, Player player, CardPlay play)
    {
        var currentJob = GetCurrentJob(player);
        if (currentJob != null)
        {
            await currentJob.OnClassChange(choiceContext, play);
            await ArisHook.OnClassChanged(choiceContext, player, currentJob.EquipmentCard);
        }
    }

    private static void HoldPlayedEquipmentCard(CardModel equipmentCard)
    {
        if (equipmentCard.Pile?.Type == PileType.Play)
        {
            var cardNode = NCard.FindOnTable(equipmentCard);
            equipmentCard.RemoveFromCurrentPile();
            if (cardNode != null)
            {
                cardNode.GetParent()?.RemoveChildSafely(cardNode);
                cardNode.QueueFreeSafelyNoPool();
            }
        }
    }

    private static async Task ReturnEquipmentCard(
        PlayerChoiceContext choiceContext,
        CardModel equipmentCard,
        PileType pileType)
    {
        if (!pileType.IsCombatPile())
        {
            return;
        }

        var combatState = equipmentCard.Owner.Creature.CombatState;
        if (combatState != null)
        {
            var returningCard = equipmentCard;
            if (returningCard.HasBeenRemovedFromState || !combatState.ContainsCard(returningCard))
            {
                returningCard = combatState.CloneCard(equipmentCard);
            }

            if (returningCard.Keywords.Contains(CardKeyword.Exhaust))
            {
                CardExhaustVfxCompat.Play(returningCard);
                await CardCmd.Exhaust(choiceContext, returningCard);
                return;
            }

            var result = await CardPileCmd.Add(returningCard, pileType, clonedBy: equipmentCard);
            if (pileType != PileType.Hand)
            {
                CardCmd.PreviewCardPileAdd(result);
            }
        }
    }

}
