using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using StS2Aris.StS2ArisCode.Cards;

namespace StS2Aris.StS2ArisCode.Mechanics;

[Flags]
public enum MomoiScenarioCardTypes
{
    Attack = 1,
    Skill = 2,
    Any = Attack | Skill
}

public enum MomoiScenarioAbility
{
    Ethereal,
    Retain,
    LoseHp,
    Damage,
    DamageAll,
    DamageRepeat,
    DamageRandom,
    DamageRandomRepeat,
    Block,
    Forge,
    Summon,
    RandomColorless,
    Intangible,
    Strength,
    Dexterity,
    Plating,
    Vigor,
    Blur,
    StrengthDown,
    LoseStrength,
    LoseDexterity,
    Poison,
    PoisonAll,
    Weak,
    WeakAll,
    Vulnerable,
    VulnerableAll,
    WeakAndVulnerable,
    Draw,
    ExhaustOther,
    ExhaustOtherRandom,
    DiscardOther,
    DiscardOtherRandom,
    Energy,
    EnergyTwo,
    EnergyNextTurn,
    EnergyNextTurnTwo,
    ExhaustAll,
    DiscardAll,
    RandomAttack,
    RandomSkill,
    RandomPower,
    RandomCard,
    Shiv,
    ShockCards,
    Upgrade,
    UpgradeRandom,
    UpgradeAll,
    Copy,
    CopyThis,
    DeckWound,
    DeckDazed,
    DeckVoid,
    DeckBurn,
    HandWound,
    HandBurn,
    DiscardBurn,
    DiscardWound,
    RetainHand,
    Exhaust,
    Charge,
    Shock,
    DoubleAttackDamageNextTurn,
    AutoPlayFromExhaust
}

public sealed record MomoiScenarioAbilityDefinition(
    MomoiScenarioAbility Ability,
    MomoiScenarioCardTypes AllowedTypes,
    int Weight,
    int Cost,
    int MinLevel,
    int MaxLevel,
    bool IsPenalty = false);

public static class MomoiScenarioGenerator
{
    public const int BaseBudget = 100;
    public const int BudgetPerRefusal = 40;

    private static readonly MomoiScenarioAbilityDefinition[] AbilityDefinitions =
    [
        new(MomoiScenarioAbility.Ethereal, MomoiScenarioCardTypes.Any, 100, -10, 1, 1, true),
        new(MomoiScenarioAbility.Retain, MomoiScenarioCardTypes.Any, 10, 16, 1, 1),
        new(MomoiScenarioAbility.LoseHp, MomoiScenarioCardTypes.Any, 50, -30, 1, 2, true),
        new(MomoiScenarioAbility.DamageAll, MomoiScenarioCardTypes.Attack, 100, 25, 1, 1),
        new(MomoiScenarioAbility.DamageRepeat, MomoiScenarioCardTypes.Attack, 100, 0, 2, 5),
        new(MomoiScenarioAbility.DamageRandom, MomoiScenarioCardTypes.Attack, 30, -24, 1, 1, true),
        new(MomoiScenarioAbility.DamageRandomRepeat, MomoiScenarioCardTypes.Attack, 20, 0, 2, 3),
        new(MomoiScenarioAbility.Block, MomoiScenarioCardTypes.Skill, 100, 12, 1, 50),
        new(MomoiScenarioAbility.Forge, MomoiScenarioCardTypes.Any, 30, 8, 1, 20),
        new(MomoiScenarioAbility.Summon, MomoiScenarioCardTypes.Any, 25, 30, 1, 5),
        new(MomoiScenarioAbility.RandomColorless, MomoiScenarioCardTypes.Any, 5, 45, 1, 3),
        new(MomoiScenarioAbility.Intangible, MomoiScenarioCardTypes.Skill, 2, 180, 1, 3),
        new(MomoiScenarioAbility.Strength, MomoiScenarioCardTypes.Skill, 10, 50, 1, 5),
        new(MomoiScenarioAbility.Dexterity, MomoiScenarioCardTypes.Skill, 10, 50, 1, 5),
        new(MomoiScenarioAbility.Plating, MomoiScenarioCardTypes.Skill, 5, 30, 1, 8),
        new(MomoiScenarioAbility.Vigor, MomoiScenarioCardTypes.Skill, 20, 10, 1, 20),
        new(MomoiScenarioAbility.Blur, MomoiScenarioCardTypes.Any, 5, 40, 1, 1),
        new(MomoiScenarioAbility.StrengthDown, MomoiScenarioCardTypes.Any, 10, 70, 1, 5),
        new(MomoiScenarioAbility.LoseStrength, MomoiScenarioCardTypes.Any, 20, -30, 1, 2, true),
        new(MomoiScenarioAbility.LoseDexterity, MomoiScenarioCardTypes.Any, 20, -30, 1, 2, true),
        new(MomoiScenarioAbility.Poison, MomoiScenarioCardTypes.Any, 15, 15, 1, 8),
        new(MomoiScenarioAbility.PoisonAll, MomoiScenarioCardTypes.Any, 10, 30, 1, 5),
        new(MomoiScenarioAbility.Weak, MomoiScenarioCardTypes.Any, 20, 20, 1, 3),
        new(MomoiScenarioAbility.WeakAll, MomoiScenarioCardTypes.Any, 10, 30, 1, 3),
        new(MomoiScenarioAbility.Vulnerable, MomoiScenarioCardTypes.Any, 20, 20, 1, 3),
        new(MomoiScenarioAbility.VulnerableAll, MomoiScenarioCardTypes.Any, 10, 30, 1, 3),
        new(MomoiScenarioAbility.WeakAndVulnerable, MomoiScenarioCardTypes.Any, 20, 50, 1, 3),
        new(MomoiScenarioAbility.Draw, MomoiScenarioCardTypes.Skill, 80, 30, 1, 4),
        new(MomoiScenarioAbility.ExhaustOther, MomoiScenarioCardTypes.Any, 20, 30, 1, 1),
        new(MomoiScenarioAbility.ExhaustOtherRandom, MomoiScenarioCardTypes.Any, 10, 20, 1, 1),
        new(MomoiScenarioAbility.DiscardOther, MomoiScenarioCardTypes.Any, 20, -5, 1, 1, true),
        new(MomoiScenarioAbility.DiscardOtherRandom, MomoiScenarioCardTypes.Any, 15, -25, 1, 1, true),
        new(MomoiScenarioAbility.Energy, MomoiScenarioCardTypes.Any, 20, 60, 1, 1),
        new(MomoiScenarioAbility.EnergyTwo, MomoiScenarioCardTypes.Any, 10, 150, 1, 1),
        new(MomoiScenarioAbility.EnergyNextTurn, MomoiScenarioCardTypes.Any, 20, 30, 1, 1),
        new(MomoiScenarioAbility.EnergyNextTurnTwo, MomoiScenarioCardTypes.Any, 10, 80, 1, 1),
        new(MomoiScenarioAbility.ExhaustAll, MomoiScenarioCardTypes.Any, 2, 50, 1, 1),
        new(MomoiScenarioAbility.DiscardAll, MomoiScenarioCardTypes.Any, 3, -50, 1, 1, true),
        new(MomoiScenarioAbility.RandomAttack, MomoiScenarioCardTypes.Any, 2, 100, 1, 1),
        new(MomoiScenarioAbility.RandomSkill, MomoiScenarioCardTypes.Any, 2, 100, 1, 1),
        new(MomoiScenarioAbility.RandomPower, MomoiScenarioCardTypes.Any, 1, 150, 1, 1),
        new(MomoiScenarioAbility.RandomCard, MomoiScenarioCardTypes.Any, 2, 70, 1, 1),
        new(MomoiScenarioAbility.ShockCards, MomoiScenarioCardTypes.Any, 10, 28, 1, 5),
        new(MomoiScenarioAbility.Upgrade, MomoiScenarioCardTypes.Any, 20, 30, 1, 1),
        new(MomoiScenarioAbility.UpgradeRandom, MomoiScenarioCardTypes.Any, 10, 20, 1, 3),
        new(MomoiScenarioAbility.UpgradeAll, MomoiScenarioCardTypes.Any, 5, 60, 1, 1),
        new(MomoiScenarioAbility.Copy, MomoiScenarioCardTypes.Skill, 2, 120, 1, 1),
        new(MomoiScenarioAbility.CopyThis, MomoiScenarioCardTypes.Any, 2, 50, 1, 1),
        new(MomoiScenarioAbility.DeckWound, MomoiScenarioCardTypes.Any, 10, -25, 1, 1, true),
        new(MomoiScenarioAbility.DeckDazed, MomoiScenarioCardTypes.Any, 10, -15, 1, 1, true),
        new(MomoiScenarioAbility.DeckVoid, MomoiScenarioCardTypes.Any, 5, -50, 1, 1, true),
        new(MomoiScenarioAbility.DeckBurn, MomoiScenarioCardTypes.Any, 10, -35, 1, 1, true),
        new(MomoiScenarioAbility.HandWound, MomoiScenarioCardTypes.Any, 10, -15, 1, 3, true),
        new(MomoiScenarioAbility.HandBurn, MomoiScenarioCardTypes.Any, 10, -25, 1, 3, true),
        new(MomoiScenarioAbility.DiscardBurn, MomoiScenarioCardTypes.Any, 10, -20, 1, 1, true),
        new(MomoiScenarioAbility.DiscardWound, MomoiScenarioCardTypes.Any, 10, -15, 1, 1, true),
        new(MomoiScenarioAbility.RetainHand, MomoiScenarioCardTypes.Any, 5, 50, 1, 1),
        new(MomoiScenarioAbility.Exhaust, MomoiScenarioCardTypes.Any, 200, -10, 1, 1, true),
        new(MomoiScenarioAbility.Charge, MomoiScenarioCardTypes.Skill, 70, 35, 1, 3),
        new(MomoiScenarioAbility.Shock, MomoiScenarioCardTypes.Attack, 70, 18, 2, 10),
        new(MomoiScenarioAbility.DoubleAttackDamageNextTurn, MomoiScenarioCardTypes.Skill, 5, 70, 1, 1),
        new(MomoiScenarioAbility.AutoPlayFromExhaust, MomoiScenarioCardTypes.Any, 5, 40, 1, 1)
    ];


    private static readonly MomoiScenarioAbility[][] Exclusions =
    [
        [MomoiScenarioAbility.DamageAll, MomoiScenarioAbility.DamageRepeat, MomoiScenarioAbility.DamageRandom, MomoiScenarioAbility.DamageRandomRepeat],
        [MomoiScenarioAbility.DamageAll, MomoiScenarioAbility.DamageRandom, MomoiScenarioAbility.DamageRandomRepeat, MomoiScenarioAbility.Weak, MomoiScenarioAbility.Vulnerable, MomoiScenarioAbility.WeakAndVulnerable],
        [MomoiScenarioAbility.Weak, MomoiScenarioAbility.WeakAll, MomoiScenarioAbility.Vulnerable, MomoiScenarioAbility.VulnerableAll, MomoiScenarioAbility.WeakAndVulnerable],
        [MomoiScenarioAbility.ExhaustOther, MomoiScenarioAbility.ExhaustOtherRandom, MomoiScenarioAbility.DiscardAll, MomoiScenarioAbility.ExhaustAll],
        [MomoiScenarioAbility.DiscardOther, MomoiScenarioAbility.DiscardOtherRandom, MomoiScenarioAbility.DiscardAll, MomoiScenarioAbility.ExhaustAll],
        [MomoiScenarioAbility.ExhaustOther,  MomoiScenarioAbility.DiscardOther,  MomoiScenarioAbility.Upgrade, MomoiScenarioAbility.Copy],
        [MomoiScenarioAbility.Energy, MomoiScenarioAbility.EnergyTwo],
        [MomoiScenarioAbility.Retain, MomoiScenarioAbility.Ethereal],
        [MomoiScenarioAbility.LoseHp],
        [MomoiScenarioAbility.Strength],
        [MomoiScenarioAbility.Dexterity],
        [MomoiScenarioAbility.LoseStrength],
        [MomoiScenarioAbility.LoseDexterity],
        [MomoiScenarioAbility.EnergyNextTurn, MomoiScenarioAbility.EnergyNextTurnTwo],
        [MomoiScenarioAbility.RandomAttack, MomoiScenarioAbility.RandomSkill, MomoiScenarioAbility.RandomPower, MomoiScenarioAbility.RandomCard],
        [MomoiScenarioAbility.Upgrade, MomoiScenarioAbility.UpgradeRandom, MomoiScenarioAbility.UpgradeAll],
        [MomoiScenarioAbility.CopyThis, MomoiScenarioAbility.Exhaust],
        [MomoiScenarioAbility.Draw, MomoiScenarioAbility.Poison, MomoiScenarioAbility.PoisonAll, MomoiScenarioAbility.Vigor],
        [MomoiScenarioAbility.ShockCards, MomoiScenarioAbility.DeckWound, MomoiScenarioAbility.DeckDazed, MomoiScenarioAbility.DeckVoid, MomoiScenarioAbility.DeckBurn, MomoiScenarioAbility.HandWound, MomoiScenarioAbility.HandBurn, MomoiScenarioAbility.DiscardBurn, MomoiScenarioAbility.DiscardWound],
        [MomoiScenarioAbility.DiscardAll, MomoiScenarioAbility.ExhaustAll, MomoiScenarioAbility.RetainHand]
    ];

    private static readonly MomoiScenarioAbility[] NeedsExhaust =
    [
        MomoiScenarioAbility.Intangible
    ];

    public static GameScenario Create(Player owner, ICombatState combatState, int refusals)
    {
        var rng = owner.RunState.Rng.CombatCardGeneration;
        var scenarioType = rng.NextBool() ? CardType.Attack : CardType.Skill;
        var scenario = combatState.CreateCard<GameScenario>(owner);
        int CostCount = rng.NextFloat() < 0.75f ? 1 : rng.NextFloat() < 0.5f ? 0 : rng.NextFloat() < 0.6f ? 2 : 3;
        scenario.Configure(scenarioType, CostCount);

        int rawBudget = BaseBudget + Math.Max(0, refusals) * BudgetPerRefusal;
        int budget = AdjustBudgetForCost(rawBudget, scenario.ScenarioCost);
        
        int abilityCount = 2;
        if (refusals > 1 && refusals <= 2)
        {
            abilityCount = MyRng(rng, 2, rng.NextBool() ? 2 : 3);
        }
        else if (refusals > 3 && refusals <= 5)
        {
            abilityCount = MyRng(rng, rng.NextFloat() < 0.75f ? 2: 3, 3);
        }
        else if (refusals > 5 && refusals <= 8)
        {
            abilityCount = MyRng(rng, rng.NextBool() ? 2 : 3, 3);
        }
        else if (refusals > 9)
        {
            abilityCount = MyRng(rng, rng.NextFloat() < 0.75f ? 3: 2, rng.NextBool() ? 3 : 4);
        }
        int penaltyCount = rng.NextFloat() < 0.25f ? 1 : 0;
        if (penaltyCount != 0 && abilityCount>=2)
        {
            abilityCount--;
        }
        List<MomoiScenarioAbility> selected = [];
        List<string> selectedDetails = [];
        global::StS2Aris.StS2ArisMain.Logger.Info(
            $"[MomoiScenarioGenerator] start refusals={refusals} type={scenarioType} cost={scenario.ScenarioCost} rawBudget={rawBudget} budget={budget} abilityCount={abilityCount} penaltyCount={penaltyCount}");
        MomoiScenarioAbilityDefinition damageDefinition = new(MomoiScenarioAbility.Damage, MomoiScenarioCardTypes.Attack, 0, 8, 1, 100);
        if (scenarioType == CardType.Attack)
        {
            int maxDamage = Math.Max(scenario.ScenarioCost * 2 + 1, budget / 2 / damageDefinition.Cost);
            int damage = rng.NextInt(scenario.ScenarioCost * 2 + 1, maxDamage + 1);
            int budgetBefore = budget;
            budget = Apply(scenario, damageDefinition, damage, budget);
            selected.Add(MomoiScenarioAbility.Damage);
            selectedDetails.Add($"{MomoiScenarioAbility.Damage}(level={damage}, budget {budgetBefore}->{budget})");
            global::StS2Aris.StS2ArisMain.Logger.Info(
                $"[MomoiScenarioGenerator] selected ability={MomoiScenarioAbility.Damage} level={damage} budgetBefore={budgetBefore} budgetAfter={budget} remainingAbilityCount={abilityCount - 1} remainingPenaltyCount={penaltyCount}");
            abilityCount--;
        }

        int extraSkillAbilities = Math.Max(0, Math.Min(4 - abilityCount, 2));
        while ((penaltyCount > 0 || abilityCount > 0) && budget > 10)
        {
            bool choosingPenalty = penaltyCount > 0;
            var options = AbilityDefinitions
                .Where(definition => definition.IsPenalty == choosingPenalty
                                     && IsAllowed(definition, scenarioType)
                                     && IsAble(definition.Ability, selected)
                                     && CanAfford(definition, budget, scenario))
                .ToList();

            if (options.Count == 0)
            {
                global::StS2Aris.StS2ArisMain.Logger.Info(
                    $"[MomoiScenarioGenerator] stop noOptions choosingPenalty={choosingPenalty} budget={budget} abilityCount={abilityCount} penaltyCount={penaltyCount} selected=[{string.Join(", ", selected)}]");
                break;
            }

            var definition = PickWeighted(rng, options);
            if (definition.Ability is MomoiScenarioAbility.DamageRepeat or MomoiScenarioAbility.DamageRandomRepeat)
            {
                budget += scenario.ScenarioDamage * damageDefinition.Cost;
            }

            int cost = GetCost(definition, scenario);
            int maxLevel = choosingPenalty || cost <= 0
                ? definition.MaxLevel
                : Math.Max(definition.MinLevel, Math.Min(definition.MaxLevel, budget / cost));
            int level = rng.NextInt(definition.MinLevel, maxLevel + 1);
            int budgetBefore = budget;
            budget = Apply(scenario, definition, level, budget);
            selected.Add(definition.Ability);
            selectedDetails.Add($"{definition.Ability}(level={level}, budget {budgetBefore}->{budget})");

            if (choosingPenalty)
            {
                penaltyCount--;
            }
            else
            {
                abilityCount--;
            }
            global::StS2Aris.StS2ArisMain.Logger.Info(
                $"[MomoiScenarioGenerator] selected ability={definition.Ability} level={level} cost={cost} maxLevel={maxLevel} isPenalty={choosingPenalty} budgetBefore={budgetBefore} budgetAfter={budget} remainingAbilityCount={abilityCount} remainingPenaltyCount={penaltyCount}");

            if (scenarioType == CardType.Skill && abilityCount == 0 && extraSkillAbilities > 0 && budget > 100)
            {
                extraSkillAbilities--;
                abilityCount++;
                global::StS2Aris.StS2ArisMain.Logger.Info(
                    $"[MomoiScenarioGenerator] addExtraSkillAbility budget={budget} remainingExtraSkillAbilities={extraSkillAbilities} abilityCount={abilityCount}");
            }
        }

        SpendRemainingBudget(scenario, budget);
        scenario.RefreshGeneratedValues();
        global::StS2Aris.StS2ArisMain.Logger.Info(
            $"[MomoiScenarioGenerator] complete type={scenario.Type} cost={scenario.ScenarioCost} finalBudget={budget} keywords=[{string.Join(", ", scenario.Keywords)}] selected=[{string.Join(", ", selectedDetails)}]");
        return scenario;
    }

    private static bool IsAllowed(MomoiScenarioAbilityDefinition definition, CardType type)
    {
        var flag = type == CardType.Attack ? MomoiScenarioCardTypes.Attack : MomoiScenarioCardTypes.Skill;
        return definition.AllowedTypes.HasFlag(flag);
    }

    private static bool IsAble(MomoiScenarioAbility ability, IReadOnlyCollection<MomoiScenarioAbility> selected)
    {
        if (selected.Contains(ability))
        {
            return false;
        }

        if (NeedsExhaust.Contains(ability) && !selected.Contains(MomoiScenarioAbility.Exhaust))
        {
            return false;
        }

        if (ability == MomoiScenarioAbility.AutoPlayFromExhaust
            && !selected.Contains(MomoiScenarioAbility.Exhaust)
            && !selected.Contains(MomoiScenarioAbility.Ethereal))
        {
            return false;
        }

        return Exclusions
            .Where(group => group.Contains(ability))
            .All(group => !group.Any(selected.Contains));
    }

    private static bool CanAfford(MomoiScenarioAbilityDefinition definition, int budget, GameScenario scenario)
    {
        if (definition.Ability == MomoiScenarioAbility.Retain && scenario.ScenarioCost <= 0)
        {
            return false;
        }

        int cost = GetCost(definition, scenario);
        return cost > 0 && cost * definition.MinLevel <= budget || definition.IsPenalty && cost < 0;
    }

    private static int GetCost(MomoiScenarioAbilityDefinition definition, GameScenario scenario)
    {
        return definition.Ability switch
        {
            MomoiScenarioAbility.DamageRepeat => Math.Max(1, scenario.ScenarioDamage * 8),
            MomoiScenarioAbility.DamageRandomRepeat => Math.Max(1, scenario.ScenarioDamage * 8 * 10 / 7),
            MomoiScenarioAbility.AutoPlayFromExhaust when scenario.ScenarioEthereal => definition.Cost * 2,
            MomoiScenarioAbility.AutoPlayFromExhaust when scenario.ScenarioExhaust => definition.Cost * 3 / 2,
            _ => definition.Cost
        };
    }

    private static MomoiScenarioAbilityDefinition PickWeighted(Rng rng, List<MomoiScenarioAbilityDefinition> options)
    {
        int totalWeight = options.Sum(static option => option.Weight);
        int roll = rng.NextInt(1, totalWeight + 1);
        foreach (var option in options)
        {
            roll -= option.Weight;
            if (roll <= 0)
            {
                return option;
            }
        }

        return options[^1];
    }

    private static int MyRng(Rng rng, int minInclusive, int maxInclusive)
    {
        return minInclusive == maxInclusive
            ? minInclusive
            : rng.NextInt(minInclusive, maxInclusive + 1);
    }

    private static int AdjustBudgetForCost(int budget, int cost)
    {
        return cost switch
        {
            <= 0 => budget * 6 / 10,
            1 => budget,
            2 => budget * 2,
            _ => budget * 3
        };
    }

    private static int Apply(GameScenario scenario, MomoiScenarioAbilityDefinition definition, int level, int budget)
    {
        switch (definition.Ability)
        {
            case MomoiScenarioAbility.Ethereal:
                scenario.ScenarioEthereal = true;
                break;
            case MomoiScenarioAbility.Retain:
                scenario.ScenarioRetain = true;
                break;
            case MomoiScenarioAbility.LoseHp:
                scenario.ScenarioLoseHp = level * 3;
                break;
            case MomoiScenarioAbility.Damage:
                scenario.ScenarioDamage = level;
                scenario.ScenarioDamageMode = (int)GameScenarioDamageMode.Single;
                break;
            case MomoiScenarioAbility.DamageAll:
                scenario.ScenarioDamageMode = (int)GameScenarioDamageMode.All;
                break;
            case MomoiScenarioAbility.DamageRepeat:
                scenario.ScenarioDamageHits = level;
                scenario.ScenarioDamageMode = (int)GameScenarioDamageMode.Repeat;
                break;
            case MomoiScenarioAbility.DamageRandom:
                scenario.ScenarioDamageMode = (int)GameScenarioDamageMode.RandomSingle;
                break;
            case MomoiScenarioAbility.DamageRandomRepeat:
                scenario.ScenarioDamageHits = level;
                scenario.ScenarioDamageMode = (int)GameScenarioDamageMode.RandomRepeat;
                break;
            case MomoiScenarioAbility.Block:
                scenario.ScenarioBlock = level;
                break;
            case MomoiScenarioAbility.Forge:
                scenario.ScenarioForge = level;
                break;
            case MomoiScenarioAbility.Summon:
                scenario.ScenarioSummon = level;
                break;
            case MomoiScenarioAbility.RandomColorless:
                scenario.ScenarioColorlessCards = level;
                break;
            case MomoiScenarioAbility.Intangible:
                scenario.ScenarioIntangible = level;
                break;
            case MomoiScenarioAbility.Strength:
                scenario.ScenarioStrength = level;
                break;
            case MomoiScenarioAbility.Dexterity:
                scenario.ScenarioDexterity = level;
                break;
            case MomoiScenarioAbility.Plating:
                scenario.ScenarioPlating = level;
                break;
            case MomoiScenarioAbility.Vigor:
                scenario.ScenarioVigor = level;
                break;
            case MomoiScenarioAbility.Blur:
                scenario.ScenarioBlur = level;
                break;
            case MomoiScenarioAbility.StrengthDown:
                scenario.ScenarioStrengthDown = level;
                break;
            case MomoiScenarioAbility.LoseStrength:
                scenario.ScenarioLoseStrength = level;
                break;
            case MomoiScenarioAbility.LoseDexterity:
                scenario.ScenarioLoseDexterity = level;
                break;
            case MomoiScenarioAbility.Poison:
                scenario.ScenarioPoison = level;
                break;
            case MomoiScenarioAbility.PoisonAll:
                scenario.ScenarioPoisonAll = level;
                break;
            case MomoiScenarioAbility.Weak:
                scenario.ScenarioWeak = level;
                break;
            case MomoiScenarioAbility.WeakAll:
                scenario.ScenarioWeakAll = level;
                break;
            case MomoiScenarioAbility.Vulnerable:
                scenario.ScenarioVulnerable = level;
                break;
            case MomoiScenarioAbility.VulnerableAll:
                scenario.ScenarioVulnerableAll = level;
                break;
            case MomoiScenarioAbility.WeakAndVulnerable:
                scenario.ScenarioWeak = level;
                scenario.ScenarioVulnerable = level;
                break;
            case MomoiScenarioAbility.Draw:
                scenario.ScenarioCards = level;
                break;
            case MomoiScenarioAbility.ExhaustOther:
                scenario.ScenarioExhaustOther = true;
                break;
            case MomoiScenarioAbility.ExhaustOtherRandom:
                scenario.ScenarioExhaustOtherRandom = true;
                break;
            case MomoiScenarioAbility.DiscardOther:
                scenario.ScenarioDiscardOther = true;
                break;
            case MomoiScenarioAbility.DiscardOtherRandom:
                scenario.ScenarioDiscardOtherRandom = true;
                break;
            case MomoiScenarioAbility.Energy:
                scenario.ScenarioEnergy = 1;
                break;
            case MomoiScenarioAbility.EnergyTwo:
                scenario.ScenarioEnergy = 2;
                break;
            case MomoiScenarioAbility.EnergyNextTurn:
                scenario.ScenarioEnergyNextTurn = 1;
                break;
            case MomoiScenarioAbility.EnergyNextTurnTwo:
                scenario.ScenarioEnergyNextTurn = 2;
                break;
            case MomoiScenarioAbility.ExhaustAll:
                scenario.ScenarioExhaustAll = true;
                break;
            case MomoiScenarioAbility.DiscardAll:
                scenario.ScenarioDiscardAll = true;
                break;
            case MomoiScenarioAbility.RandomAttack:
                scenario.ScenarioRandomAttack = true;
                break;
            case MomoiScenarioAbility.RandomSkill:
                scenario.ScenarioRandomSkill = true;
                break;
            case MomoiScenarioAbility.RandomPower:
                scenario.ScenarioRandomPower = true;
                break;
            case MomoiScenarioAbility.RandomCard:
                scenario.ScenarioRandomCard = true;
                break;
            case MomoiScenarioAbility.Shiv:
                scenario.ScenarioShiv = level;
                break;
            case MomoiScenarioAbility.ShockCards:
                scenario.ScenarioShockCards = level;
                break;
            case MomoiScenarioAbility.Upgrade:
                scenario.ScenarioUpgrade = true;
                break;
            case MomoiScenarioAbility.UpgradeRandom:
                scenario.ScenarioUpgradeRandom = level;
                break;
            case MomoiScenarioAbility.UpgradeAll:
                scenario.ScenarioUpgradeAll = true;
                break;
            case MomoiScenarioAbility.Copy:
                scenario.ScenarioCopy = true;
                break;
            case MomoiScenarioAbility.CopyThis:
                scenario.ScenarioCopyThis = true;
                break;
            case MomoiScenarioAbility.DeckWound:
                scenario.ScenarioDeckWound = level;
                break;
            case MomoiScenarioAbility.DeckDazed:
                scenario.ScenarioDeckDazed = level;
                break;
            case MomoiScenarioAbility.DeckVoid:
                scenario.ScenarioDeckVoid = level;
                break;
            case MomoiScenarioAbility.DeckBurn:
                scenario.ScenarioDeckBurn = level;
                break;
            case MomoiScenarioAbility.HandWound:
                scenario.ScenarioHandWound = level;
                break;
            case MomoiScenarioAbility.HandBurn:
                scenario.ScenarioHandBurn = level;
                break;
            case MomoiScenarioAbility.DiscardBurn:
                scenario.ScenarioDiscardBurn = level;
                break;
            case MomoiScenarioAbility.DiscardWound:
                scenario.ScenarioDiscardWound = level;
                break;
            case MomoiScenarioAbility.RetainHand:
                scenario.ScenarioRetainHand = true;
                break;
            case MomoiScenarioAbility.Exhaust:
                scenario.ScenarioExhaust = true;
                break;
            case MomoiScenarioAbility.Charge:
                scenario.ScenarioCharge = level;
                break;
            case MomoiScenarioAbility.Shock:
                scenario.ScenarioShock = level;
                break;
            case MomoiScenarioAbility.DoubleAttackDamageNextTurn:
                scenario.ScenarioDoubleAttackDamageNextTurn = true;
                break;
            case MomoiScenarioAbility.AutoPlayFromExhaust:
                scenario.ScenarioAutoPlayFromExhaust = true;
                break;
        }

        scenario.RefreshGeneratedValues();
        return budget - GetCost(definition, scenario) * level;
    }

    private static void SpendRemainingBudget(GameScenario scenario, int budget)
    {
        if (scenario.Type == CardType.Attack && scenario.ScenarioDamage > 0)
        {
            int hits = Math.Max(1, scenario.ScenarioDamageHits);
            int cost = 8 * hits;
            if (budget > cost)
            {
                scenario.ScenarioDamage += budget / cost;
            }
            return;
        }

        if (budget <= 10)
        {
            return;
        }

        if (scenario.ScenarioBlock > 0)
        {
            scenario.ScenarioBlock += budget / 12;
        }
        else if (scenario.ScenarioForge > 0)
        {
            scenario.ScenarioForge = Math.Min(20, scenario.ScenarioForge + budget / 8);
        }
        else if (scenario.ScenarioSummon > 0)
        {
            scenario.ScenarioSummon = Math.Min(5, scenario.ScenarioSummon + budget / 30);
        }
        else if (scenario.ScenarioCards > 0)
        {
            scenario.ScenarioCards = Math.Min(4, scenario.ScenarioCards + budget / 30);
        }
        else if (scenario.ScenarioColorlessCards > 0)
        {
            scenario.ScenarioColorlessCards = Math.Min(3, scenario.ScenarioColorlessCards + budget / 45);
        }
        else if (scenario.ScenarioCharge > 0)
        {
            scenario.ScenarioCharge = Math.Min(3, scenario.ScenarioCharge + budget / 35);
        }
        else if (scenario.ScenarioShockCards > 0)
        {
            scenario.ScenarioShockCards = Math.Min(5, scenario.ScenarioShockCards + budget / 28);
        }
        else if (scenario.ScenarioStrength > 0)
        {
            scenario.ScenarioStrength = Math.Min(5, scenario.ScenarioStrength + budget / 50);
        }
        else if (scenario.ScenarioDexterity > 0)
        {
            scenario.ScenarioDexterity = Math.Min(5, scenario.ScenarioDexterity + budget / 50);
        }
        else if (!scenario.HasPositiveSkillEffect)
        {
            scenario.ScenarioBlock = Math.Max(6, budget / 12);
        }
    }
}
