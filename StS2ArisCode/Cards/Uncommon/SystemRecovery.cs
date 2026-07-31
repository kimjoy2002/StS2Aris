using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class SystemRecovery() : StS2ArisCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("SelfDamage", 2m),
        new PowerVar<SystemRecoveryPower>(1m)
    ];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var power = await PowerCmd.Apply<SystemRecoveryPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["SystemRecoveryPower"].BaseValue,
            Owner.Creature,
            this);
        power?.QueueRecoveryCheck();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["SystemRecoveryPower"].UpgradeValueBy(1m);
    }
}
