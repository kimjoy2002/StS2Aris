using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Hooks;
using StS2Aris.StS2ArisCode.Powers;
using StS2Aris.StS2ArisCode.Relics;

namespace StS2Aris.StS2ArisCode.Mechanics;

public static class ArisCharge
{
    private static readonly SpireField<CardModel, int> ChargeSpentField = new(() => 0);
    private static readonly SpireField<CardModel, bool> EnergyWasEmptyBeforeSpendField = new(() => false);
    public static int OverloadsThisCombat { get; private set; }

    public static int Get(Player player) => player.Creature.GetPower<ChargePower>()?.Amount ?? 0;

    public static int GetSpent(CardModel card) => ChargeSpentField.Get(card);

    public static bool WasEnergyEmptyBeforeSpend(CardModel card) => EnergyWasEmptyBeforeSpendField.Get(card);

    public static void SetEnergyWasEmptyBeforeSpend(CardModel card, bool value) => EnergyWasEmptyBeforeSpendField.Set(card, value);

    public static int GetExpectedSpent(CardModel card)
    {
        Player? player = card.Owner;
        PlayerCombatState? state = player?.PlayerCombatState;
        if (player == null || state == null || !CanSpendCharge(card))
        {
            return 0;
        }

        int totalEnergyCost = GetEnergyAmountToSpend(card, includeChargeForX: true);
        int energySpent = Math.Min(totalEnergyCost, state.Energy);
        int remainingEnergyCost = totalEnergyCost - energySpent;
        return Math.Min(remainingEnergyCost, Get(player));
    }

    public static int GetSpentOrExpected(CardModel card)
    {
        int spent = GetSpent(card);
        return spent > 0 ? spent : GetExpectedSpent(card);
    }

    public static void ClearSpent(CardModel card)
    {
        ChargeSpentField.Set(card, 0);
    }

    public static bool IsOverloadState(Player? player) => player?.PlayerCombatState?.Energy <= 0;

    public static bool IsOverloadAvailable(Player? player) =>
        IsOverloadState(player) || player?.Creature.GetPower<JobKeiPower>() != null;

    public static bool WillBeOverloadAfterSpending(CardModel card)
    {
        Player? player = card.Owner;
        PlayerCombatState? state = player?.PlayerCombatState;
        if (player == null || state == null)
        {
            return false;
        }

        if (player.Creature.GetPower<JobKeiPower>() != null)
        {
            return true;
        }

        int energySpent = Math.Min(GetEnergyAmountToSpend(card, includeChargeForX: CanSpendCharge(card)), state.Energy);
        return state.Energy - energySpent <= 0;
    }

    public static void ResetOverloadCount()
    {
        OverloadsThisCombat = 0;
    }

    public static void NotifyOverload()
    {
        OverloadsThisCombat++;
    }

    public static async Task NotifyOverload(PlayerChoiceContext choiceContext, Player? player, CardModel? cardSource)
    {
        NotifyOverload();

        if (player?.Creature.GetPower<CounterStopPower>() is { } counterStopPower)
        {
            await counterStopPower.OnOverloadTriggered(choiceContext, cardSource);
        }

        if (player != null)
        {
            await ArisHook.OnOverloadTriggered(choiceContext, player, cardSource);
        }
    }

    public static bool HasOutput(CardModel card) => card is IArisOutputCard || card.Keywords.Contains(ArisKeywords.Output);

    public static bool CanSpendCharge(CardModel card) => !HasOutput(card)
        || (card.IsMutable && card.Owner?.Relics.OfType<Joystick>().Any(relic => relic.CanIgnoreOutput(card)) == true);

    public static bool HasEnoughResourcesFor(PlayerCombatState state, Player player, CardModel card, out UnplayableReason reason)
    {
        bool canSpendCharge = CanSpendCharge(card);
        int energyCost = GetEnergyAmountToSpend(card, includeChargeForX: canSpendCharge);
        int energy = state.Energy;
        int charge = canSpendCharge ? Get(player) : 0;
        int remainingEnergyCost = Math.Max(0, energyCost - energy - charge);
        int starCost = Math.Max(0, card.GetStarCostWithModifiers());

        if (remainingEnergyCost > 0 && card.CombatState != null && Hook.ShouldPayExcessEnergyCostWithStars(card.CombatState, player))
        {
            starCost += remainingEnergyCost * 2;
            remainingEnergyCost = 0;
        }

        reason = UnplayableReason.None;
        if (remainingEnergyCost > 0)
        {
            reason |= UnplayableReason.EnergyCostTooHigh;
        }

        if (starCost > state.Stars)
        {
            reason |= UnplayableReason.StarCostTooHigh;
        }

        return reason == UnplayableReason.None;
    }

    public static async Task<(int energySpent, int starsSpent)> SpendResources(CardModel card)
    {
        Player? player = card.Owner;
        PlayerCombatState? state = player?.PlayerCombatState;
        ICombatState? combatState = card.CombatState ?? player?.Creature.CombatState;
        if (player == null || state == null || combatState == null)
        {
            ChargeSpentField.Set(card, 0);
            EnergyWasEmptyBeforeSpendField.Set(card, false);
            card.LastStarsSpent = 0;
            return (0, 0);
        }

        EnergyWasEmptyBeforeSpendField.Set(card, state.Energy <= 0);

        bool canSpendCharge = CanSpendCharge(card);
        int totalEnergyCost = GetEnergyAmountToSpend(card, includeChargeForX: canSpendCharge);
        int energySpent = Math.Min(totalEnergyCost, state.Energy);
        int remainingEnergyCost = totalEnergyCost - energySpent;
        int chargeSpent = canSpendCharge ? Math.Min(remainingEnergyCost, Get(player)) : 0;
        remainingEnergyCost -= chargeSpent;

        int starsSpent = Math.Max(0, card.GetStarCostWithModifiers());
        if (remainingEnergyCost > 0 && Hook.ShouldPayExcessEnergyCostWithStars(combatState, player))
        {
            starsSpent += remainingEnergyCost * 2;
            remainingEnergyCost = 0;
        }

        if (card.EnergyCost.CostsX)
        {
            card.EnergyCost.CapturedXValue = totalEnergyCost - remainingEnergyCost;
        }

        ChargeSpentField.Set(card, chargeSpent);

        if (chargeSpent > 0 && HasOutput(card))
        {
            player.Relics.OfType<Joystick>().FirstOrDefault(relic => relic.CanIgnoreOutput(card))?.ConsumeOutputBypass();
        }

        if (energySpent > 0)
        {
            CombatManager.Instance.History.EnergySpent(combatState, energySpent, player);
            state.LoseEnergy(energySpent);
        }

        await Hook.AfterEnergySpent(combatState, card, energySpent);

        if (chargeSpent > 0 && player.Creature.GetPower<ChargePower>() is { } chargePower)
        {
            await PowerCmd.ModifyAmount(new ThrowingPlayerChoiceContext(), chargePower, -chargeSpent, player.Creature, card);
        }

        card.LastStarsSpent = starsSpent;
        if (starsSpent > 0)
        {
            state.LoseStars(starsSpent);
            await Hook.AfterStarsSpent(combatState, starsSpent, player);
        }

        return (energySpent, starsSpent);
    }

    private static int GetEnergyAmountToSpend(CardModel card, bool includeChargeForX)
    {
        if (!card.EnergyCost.CostsX)
        {
            return card.EnergyCost.GetAmountToSpend();
        }

        Player? player = card.Owner;
        if (player == null)
        {
            return 0;
        }

        int energy = player.PlayerCombatState?.Energy ?? 0;
        return includeChargeForX ? energy + Get(player) : energy;
    }
}
