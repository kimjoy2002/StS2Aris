using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace StS2Aris.StS2ArisCode.Cards;

internal sealed class GameScenarioDamageVar(decimal damage, ValueProp props) : DamageVar(damage, props)
{
    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        try
        {
            base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        }
        catch (NullReferenceException) when (card.Enchantment != null)
        {
            PreviewValue = BaseValue;
            if (!card.IsEnchantmentPreview)
            {
                EnchantedValue = BaseValue;
            }
        }
    }
}
