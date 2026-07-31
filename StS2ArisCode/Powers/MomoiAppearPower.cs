using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Powers;

public sealed class MomoiAppearPower : StS2ArisPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int Refusals { get; set; }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        var combatState = CombatState;
        if (player != Owner.Player || Owner.IsDead || combatState == null)
        {
            return;
        }

        var scenarios = Enumerable.Range(0, Math.Max(1, Amount))
            .Select(_ => MomoiScenarioGenerator.Create(player, combatState, Refusals))
            .Cast<CardModel>()
            .ToList();
        foreach (var gameArtPower in player.Creature.GetPowerInstances<GameArtPower>())
        {
            foreach (var scenario in scenarios)
            {
                GameArtPower.ApplyToGeneratedCard(scenario, gameArtPower.Amount);
            }
        }

        var refuse = combatState.CreateCard<ChooseRefuse>(player);
        scenarios.Add(refuse);

        var selected = await ChooseScenario(choiceContext, scenarios, player);

        if (selected is GameScenario selectedScenario)
        {
            await CardPileCmd.AddGeneratedCardToCombat(selectedScenario, PileType.Hand, player);
            return;
        }

        Refusals++;
        InvokeDisplayAmountChanged();
    }

    private static async Task<CardModel?> ChooseScenario(PlayerChoiceContext choiceContext, IReadOnlyList<CardModel> choices, Player player)
    {
        if (choices.Count <= 3)
        {
            return await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, player);
        }

        return (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            player,
            new CardSelectorPrefs(new LocString("cards", "STS2ARIS-MOMOI_APPEAR.selectionScreenPrompt"), 1))).FirstOrDefault();
    }
}
