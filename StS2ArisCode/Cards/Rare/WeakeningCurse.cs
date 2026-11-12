using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Cards;
[Pool(typeof(StS2ArisCardPool))]
public class WeakeningCurse() : StS2ArisCard(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Magic", 9m),
        new DynamicVar("ReturnTurns", 3m)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        var strengthBefore = play.Target.GetPower<StrengthPower>()?.Amount ?? 0;
        await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target, -DynamicVars["Magic"].IntValue, Owner.Creature, this);
        var strengthAfter = play.Target.GetPower<StrengthPower>()?.Amount ?? 0;
        var reducedAmount = strengthBefore - strengthAfter;
        if (reducedAmount <= 0)
        {
            return;
        }

        var restoreAmount = reducedAmount / DynamicVars["ReturnTurns"].IntValue;
        var restore = await PowerCmd.Apply<RestoreStrPower>(choiceContext, play.Target, restoreAmount, Owner.Creature, this);
        if (restore != null)
        {
            restore.TurnsRemaining = DynamicVars["ReturnTurns"].IntValue;
        }
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override void OnUpgrade()
    {
        DynamicVars["Magic"].UpgradeValueBy(3m);
    }
}




