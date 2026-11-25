using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Random;
using StS2Aris.StS2ArisCode.Utils;
using CoreCards = MegaCrit.Sts2.Core.Models.Cards;

namespace StS2Aris.StS2ArisCode.Mechanics;

public static class StrategyGuideCardPool
{
    private static readonly Type[] Common =
    [
        typeof(CoreCards.BurningPact),
        typeof(CoreCards.CalculatedGamble),
        typeof(CoreCards.Bulwark),
        typeof(CoreCards.Putrefy),
        typeof(CoreCards.BulkUp),
        typeof(CoreCards.Impervious),
        typeof(CoreCards.Adrenaline),
        typeof(CoreCards.Tyranny),
        typeof(CoreCards.Hang),
        typeof(CoreCards.EchoForm)
    ];

    private static readonly Dictionary<Type, Type[]> BossSpecific = new()
    {
        [typeof(AeonglassBoss)] =
        [
            typeof(CoreCards.SecondWind),
            typeof(CoreCards.Expose),
            typeof(CoreCards.Charge),
            typeof(CoreCards.Lethality),
            typeof(CoreCards.Compact),
            typeof(CoreCards.FiendFire),
            typeof(CoreCards.ShadowStep),
            typeof(CoreCards.Guards),
            typeof(CoreCards.FlakCannon),
            typeof(CoreCards.ReaperForm)
        ],
        [typeof(TestSubjectBoss)] =
        [
            typeof(CoreCards.FightMe),
            typeof(CoreCards.Backstab),
            typeof(CoreCards.Supermassive),
            typeof(CoreCards.EnfeeblingTouch),
            typeof(CoreCards.Ftl),
            typeof(CoreCards.Barricade),
            typeof(CoreCards.Buffer),
            typeof(CoreCards.SerpentForm),
            typeof(CoreCards.Eradicate),
            typeof(CoreCards.AdaptiveStrike)
        ],
        [typeof(QueenBoss)] =
        [
            typeof(CoreCards.BattleTrance),
            typeof(CoreCards.WellLaidPlans),
            typeof(CoreCards.SpectrumShift),
            typeof(CoreCards.EnfeeblingTouch),
            typeof(CoreCards.Glacier),
            typeof(CoreCards.CrimsonMantle),
            typeof(CoreCards.Malaise),
            typeof(CoreCards.ForegoneConclusion),
            typeof(CoreCards.SharedFate),
            typeof(CoreCards.Reboot)
        ]
    };

    public static IReadOnlyList<CardModel> CreateCards(Player player, int count, bool upgraded)
    {
        var selectedTypes = SelectCardTypes(player.Creature.CombatState, count, player.RunState.Rng.CombatCardGeneration);
        return selectedTypes.Select(type => CreateCard(player, type, upgraded)).ToList();
    }

    private static List<Type> SelectCardTypes(ICombatState? combatState, int count, Rng rng)
    {
        BossSpecific.TryGetValue(combatState?.Encounter?.GetType() ?? typeof(void), out var bossPool);
        var selected = new List<Type>();
        var common = Common.ToList();
        var boss = (bossPool ?? []).ToList();

        for (var i = 0; i < count; i++)
        {
            var preferBoss = boss.Count > 0 && rng.NextFloat() < 0.8f;
            var source = preferBoss ? boss : common;
            if (source.Count == 0)
            {
                source = preferBoss ? common : boss;
            }

            if (source.Count == 0)
            {
                break;
            }

            var type = rng.NextItem(source);
            if (type == null)
            {
                break;
            }

            selected.Add(type);
            common.Remove(type);
            boss.Remove(type);
        }

        return selected;
    }

    private static CardModel CreateCard(Player player, Type type, bool upgraded)
    {
        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(type));
        var card = player.Creature.CombatState?.CreateCard(canonical, player) ?? player.RunState.CreateCard(canonical, player);
        if (upgraded && card.IsUpgradable)
        {
            CardCmdCompat.UpgradeInternal(card);
            card.FinalizeUpgradeInternal();
        }

        return card;
    }
}
