using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Utils;

namespace StS2Aris.StS2ArisCode.Potions;

[Pool(typeof(StS2ArisPotionPool))]
public sealed class ClassChangeElixir : StS2ArisPotion
{
    public override PotionRarity Rarity => PotionRarity.Rare;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Repeat", 2m)
    ];

    public override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Equipment),
        HoverTipFactory.FromKeyword(ArisKeywords.ClassChange)
    ];

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        var job = ArisEquipment.GetCurrentJob(Owner);
        var equipment = job?.EquipmentCard;
        if (equipment == null)
        {
            return;
        }

        NCombatRoom.Instance?.PlaySplashVfx(Owner.Creature, new Color("a8e9df"));
        for (var i = 0; i < DynamicVars["Repeat"].IntValue; i++)
        {
            if (Owner.Creature.IsDead || CombatManager.Instance.IsOverOrEnding ||
                ArisEquipment.GetCurrentJob(Owner) != job)
            {
                break;
            }

            // Supply the equipped card as the effect source without playing or moving it.
            var play = CardPlayCompat.CreateForEffect(equipment);
            await ArisEquipment.TriggerClassChange(choiceContext, Owner, play);
        }
    }
}
