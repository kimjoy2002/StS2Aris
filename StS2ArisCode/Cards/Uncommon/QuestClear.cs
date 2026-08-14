using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class QuestClear() : StS2ArisCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    private const int BaseBlock = 8;
    private const int UpgradeBlock = 3;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest)];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Magic", 2m),
        ..MakeCalculatedBlock("CalculatedBlock", BaseBlock, (card, _) =>
        {
            if (!card.IsMutable)
            {
                return 0m;
            }

            var owner = card.Owner;
            return owner == null
                ? 0m
                : ArisQuestUtils.CountCompletedQuests(owner) * card.DynamicVars["Magic"].IntValue;
        })
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        decimal block = DynamicVars["CalculatedBlockBase"].BaseValue
                        + DynamicVars["CalculatedBlockExtra"].BaseValue
                        * ArisQuestUtils.CountCompletedQuests(Owner)
                        * DynamicVars["Magic"].IntValue;
        await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["CalculatedBlockBase"].UpgradeValueBy(UpgradeBlock);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        if (!IsMutable || CombatState != null || Pile?.Type != PileType.Deck)
        {
            return;
        }

        var owner = Owner;
        if (owner == null)
        {
            return;
        }

        decimal block = DynamicVars["CalculatedBlockBase"].BaseValue
                        + DynamicVars["CalculatedBlockExtra"].BaseValue
                        * ArisQuestUtils.CountCompletedQuests(owner)
                        * DynamicVars["Magic"].IntValue;
        var displayBlock = new BlockVar("CalculatedBlock", block, ValueProp.Move);
        if (DynamicVars["CalculatedBlockBase"].WasJustUpgraded)
        {
            displayBlock.BaseValue -= UpgradeBlock;
            displayBlock.UpgradeValueBy(UpgradeBlock);
        }

        description.Add(displayBlock);
    }
}
