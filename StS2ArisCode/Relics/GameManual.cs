using BaseLib.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Relics;
public class GameManual : StS2ArisRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public override bool HasUponPickupEffect => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    public override async Task AfterObtained()
    {
        CardCreationOptions options = CardCreationOptions.ForNonCombatWithUniformOdds(
            ModelDb.AllCardPools,
            card => (card.Type == CardType.Quest || card is StS2ArisCard { IsArisQuest: true })
                    && (card is not SpoilsMap || Owner.RunState.CurrentActIndex == 0));
        List<Reward> rewards = [];
        for (int i = 0; i < DynamicVars.Cards.IntValue; i++)
        {
            rewards.Add(CardRewardCompat.Create(options, 3, Owner));
        }

        await RewardsCmd.OfferCustom(Owner, rewards);
    }
}
