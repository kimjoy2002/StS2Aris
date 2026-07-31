using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(TokenCardPool))]
public class CleanUp() : StS2ArisCard(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(34, ValueProp.Move)];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        ArgumentNullException.ThrowIfNull(play.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play).WithHitFx("vfx/vfx_heavy_blunt").Targeting(play.Target).Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    public static async Task<CardModel?> CreateInDrawPile(Player owner, ICombatState combatState, bool upgraded)
    {
        if (CombatManager.Instance.IsOverOrEnding)
        {
            return null;
        }

        var card = combatState.CreateCard<CleanUp>(owner);
        if (upgraded)
        {
            CardCmd.Upgrade(card);
        }

        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, owner, CardPilePosition.Random);
        return card;
    }
}
