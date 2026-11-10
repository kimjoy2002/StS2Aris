using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.CardModels;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Relics;

public class MaskedCommander : StS2ArisRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Equipment),
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner || Owner.PlayerCombatState?.TurnNumber != 1
            || Owner.Creature.CombatState == null)
        {
            return;
        }

        var options = ModelDb.CardPool<StS2ArisCardPool>()
            .GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
            .Where(card => card is IArisEquipmentCard
                && card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare
                && card.CanBeGeneratedByModifiers)
            .OrderBy(card => card.Id.Entry, StringComparer.Ordinal)
            .ToList();
        var card = CardFactory.GetDistinctForCombat(Owner, options, 1,
            Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card == null)
        {
            return;
        }

        Flash();
        card.AddKeyword(CardKeyword.Exhaust);
        card.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }
}
