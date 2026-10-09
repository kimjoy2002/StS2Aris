using BaseLib.Patches.Localization;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace StS2Aris.StS2ArisCode.Powers;

public sealed class SystemRecoveryPower : StS2ArisPower, IAddDumbVariablesToPowerDescription
{
    private const int HandThreshold = 2;
    internal const int SelfDamage = 2;
    private bool _triggeredThisTurn;
    private bool _turnEnding;
    private bool _triggerPending;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            _triggeredThisTurn = false;
            _turnEnding = false;
            _triggerPending = false;
        }

        return Task.CompletedTask;
    }

    public override Task BeforeSideTurnEndVeryEarly(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            _turnEnding = true;
            _triggerPending = false;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        var player = Owner.Player;
        if (_triggeredThisTurn
            || _turnEnding
            || Owner.IsDead
            || player == null
            || card.Owner != player
            || oldPileType != PileType.Hand
            || player.PlayerCombatState?.Hand.Cards.Count > HandThreshold)
        {
            return;
        }

        if (card.Pile?.Type == PileType.Play || CombatManager.Instance.IsExecutingCardOrPotionEffect(player))
        {
            _triggerPending = true;
            return;
        }

        await TriggerRecovery(new ThrowingPlayerChoiceContext());
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!_triggerPending || !cardPlay.IsLastInSeries || cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        await TriggerRecovery(choiceContext);
    }

    public override async Task AfterPotionUsed(PotionModel potion, Creature? target)
    {
        if (_triggerPending && potion.Owner?.Creature == Owner)
        {
            await TriggerRecovery(new ThrowingPlayerChoiceContext());
        }
    }

    public void QueueRecoveryCheck()
    {
        if (!_triggeredThisTurn && !_turnEnding && !Owner.IsDead)
        {
            _triggerPending = true;
        }
    }

    private async Task TriggerRecovery(PlayerChoiceContext choiceContext)
    {
        _triggerPending = false;
        var player = Owner.Player;
        if (_triggeredThisTurn
            || _turnEnding
            || Owner.IsDead
            || player?.PlayerCombatState == null
            || player.PlayerCombatState.Hand.Cards.Count > HandThreshold)
        {
            return;
        }

        _triggeredThisTurn = true;
        Flash();

        await CreatureCmd.Damage(choiceContext, Owner, SelfDamage, ValueProp.Unpowered, Owner);
        if (!Owner.IsDead)
        {
            await CardPileCmd.Draw(choiceContext, Amount, player);
        }
    }

    public void AddDumbVariablesToPowerDescription(LocString description)
    {
        description.Add("SelfDamage", SelfDamage);
        description.Add("HandThreshold", HandThreshold);
        description.Add("Amount", Amount);
    }
}
