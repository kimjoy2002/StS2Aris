using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class EquipHotkey() : StS2ArisCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Innate, CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromKeyword(ArisKeywords.Equipment);
            if (IsUpgraded)
            {
                yield return HoverTipFactory.FromKeyword(CardKeyword.Retain);
            }
        }
    }

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var equipmentCards = PileType.Draw.GetPile(Owner).Cards
            .OfType<StS2ArisEquipmentCard>()
            .Cast<CardModel>()
            .ToList();

        foreach (var equipmentCard in equipmentCards)
        {
            await CardPileCmd.Add(equipmentCard, PileType.Hand);
        }

        if (!IsUpgraded)
        {
            return;
        }

        foreach (var equipmentCard in PileType.Hand.GetPile(Owner).Cards.OfType<StS2ArisEquipmentCard>())
        {
            CardCmd.ApplyKeyword(equipmentCard, CardKeyword.Retain);
        }
    }
}
