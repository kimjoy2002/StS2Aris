using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Relics;

public abstract class ClassAltarRelic : StS2ArisRelic
{
    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool HasUponPickupEffect => true;

    protected abstract CardPoolModel? RewardCardPool { get; }
    protected abstract CardModel CreateEquipment(Player owner);

    public override async Task AfterObtained()
    {
        CardModel? superNova = PileType.Deck.GetPile(Owner).Cards.FirstOrDefault(card => card is SuperNova);
        if (superNova != null)
        {
            CardModel equipment = CreateEquipment(Owner);
            while (equipment.IsUpgradable && equipment.CurrentUpgradeLevel < superNova.CurrentUpgradeLevel)
            {
                equipment.UpgradeInternal();
                equipment.FinalizeUpgradeInternal();
            }

            await CardCmd.Transform(superNova, equipment, CardPreviewStyle.EventLayout);
        }

        CardPoolModel? pool = RewardCardPool;
        if (pool == null)
        {
            return;
        }

        CardCreationOptions options = CardCreationOptions.ForNonCombatWithDefaultOdds([pool])
            .WithFlags(CardCreationFlags.NoCardPoolModifications);
        await RewardsCmd.OfferCustom(Owner,
        [
            CardRewardCompat.Create(options, 3, Owner),
            CardRewardCompat.Create(options, 3, Owner)
        ]);
    }

    public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
    {
        CardPoolModel? pool = RewardCardPool;
        if (Owner != player || pool == null ||
            options.Flags.HasFlag(CardCreationFlags.NoCardPoolModifications) ||
            !options.Flags.HasFlag(CardCreationFlags.IsCardReward))
        {
            return options;
        }

        return CardCreationOptionsCompat.WithCardPools(options, options.CardPools.Union([pool]));
    }
}
