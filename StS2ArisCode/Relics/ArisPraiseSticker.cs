using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Relics;

public class ArisPraiseSticker : StS2ArisRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(4m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [HoverTipFactory.FromKeyword(ArisKeywords.Quest)];

    public async Task OnQuestCompleted(int count)
    {
        if (count <= 0 || Owner.Creature.IsDead)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner.Creature, DynamicVars["Heal"].BaseValue * count);
    }
}
