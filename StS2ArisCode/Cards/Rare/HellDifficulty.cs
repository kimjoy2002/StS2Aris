using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Keywords;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class HellDifficulty() : ArisQuestCard<StrategyGuide>(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override int QuestGoal => 1;

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Quest),
        HoverTipFactory.FromKeyword(ArisKeywords.Reward),
        HoverTipFactory.FromCard<StrategyGuide>(IsUpgraded)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(3)];

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    public override async Task BeforeCombatStart()
    {
        if (Pile?.Type == PileType.Deck
            && Owner.RunState.CurrentActIndex >= 2
            && Owner.PlayerCombatState != null
            && Owner.Creature.CombatState is { RunState.CurrentRoom: CombatRoom { RoomType: RoomType.Boss } } combatState)
        {
            await CompleteQuest();

            foreach (var combatCopy in Owner.PlayerCombatState.AllCards.OfType<HellDifficulty>().Where(card => !card.HasBeenRemovedFromState).ToList())
            {
                var reward = combatState.CreateCard<StrategyGuide>(Owner);
                if (combatCopy.IsUpgraded)
                {
                    reward.UpgradeInternal();
                    reward.FinalizeUpgradeInternal();
                }

                combatCopy.CopyEnchantmentToReward(reward);
                await CardCmd.Transform(combatCopy, reward, CardPreviewStyle.None);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
