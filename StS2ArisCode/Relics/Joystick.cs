using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Relics;

public class Joystick : StS2ArisRelic
{
    private bool _usedThisTurn;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ArisKeywords.Output)];

    [SavedProperty]
    public bool UsedThisTurn
    {
        get => _usedThisTurn;
        set
        {
            AssertMutable();
            _usedThisTurn = value;
            Status = value ? RelicStatus.Normal : RelicStatus.Active;
        }
    }

    public bool CanIgnoreOutput(CardModel card) =>
        !UsedThisTurn && !card.EnergyCost.CostsX && card.IsMutable && card.Owner == Owner;

    public void ConsumeOutputBypass()
    {
        UsedThisTurn = true;
        Flash();
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is CombatRoom)
        {
            UsedThisTurn = false;
        }
        return Task.CompletedTask;
    }

    public override Task AfterEnergyReset(Player player)
    {
        if (player == Owner)
        {
            UsedThisTurn = false;
        }
        return Task.CompletedTask;
    }
}
