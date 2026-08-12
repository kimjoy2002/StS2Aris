using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Cards;
[Pool(typeof(TokenCardPool))]
public class NeatCompression() : StS2ArisCard(0, CardType.Power, CardRarity.Token, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Reward)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "PowerUp", base.Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<NeatCompressionPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);

        if (IsUpgraded)
        {
            var upgradeTarget = PileType.Deck.GetPile(Owner).Cards
                .Where(card => card.IsUpgradable && card.Id != Id)
                .ToList()
                .TakeRandom(1, Owner.RunState.Rng.CombatCardSelection)
                .FirstOrDefault();

            if (upgradeTarget != null)
            {
                CardCmd.Upgrade(upgradeTarget);
            }
        }

        if (DeckVersion != null)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion);
            DeckVersion = null;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];
}
