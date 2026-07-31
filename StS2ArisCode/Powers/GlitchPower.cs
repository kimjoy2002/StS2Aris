using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Powers;

public sealed class GlitchPower : StS2ArisPower
{
    private sealed class Data
    {
        public int CardsRepeatedThisTurn;
    }

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override int DisplayAmount => Math.Max(0, Amount - GetInternalData<Data>().CardsRepeatedThisTurn);

    protected override object InitInternalData()
    {
        return new Data();
    }

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (!CanRepeat(card))
        {
            return playCount;
        }

        return playCount + 1;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        GetInternalData<Data>().CardsRepeatedThisTurn++;
        InvokeDisplayAmountChanged();
        Flash();

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            GetInternalData<Data>().CardsRepeatedThisTurn = 0;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }

    private bool CanRepeat(CardModel card)
    {
        return card.Owner.Creature == Owner
               && card is not Glitch
               && GetInternalData<Data>().CardsRepeatedThisTurn < Amount
               && ArisCharge.WasEnergyEmptyBeforeSpend(card);
    }
}
