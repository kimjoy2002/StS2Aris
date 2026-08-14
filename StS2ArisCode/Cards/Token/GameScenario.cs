using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Extensions;
using StS2Aris.StS2ArisCode.Powers;
using StS2Aris.StS2ArisCode.Utils;
using Void = MegaCrit.Sts2.Core.Models.Cards.Void;

namespace StS2Aris.StS2ArisCode.Cards;

public enum GameScenarioDamageMode
{
    Single,
    All,
    Repeat,
    RandomSingle,
    RandomRepeat
}

[Pool(typeof(TokenCardPool))]
public class GameScenario() : StS2ArisCard(1, CardType.Skill, CardRarity.Token, TargetType.Self)
{
    private static int _chronicleCount;

    public static void ResetChronicleCount()
    {
        _chronicleCount = 0;
    }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioType { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioCost { get; set; } = 1;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDamage { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDamageMode { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDamageHits { get; set; } = 1;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioBlock { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioCards { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioForge { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioSummon { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioColorlessCards { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioCharge { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioShockCards { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioShock { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioWeak { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioWeakAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioVulnerable { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioVulnerableAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioStrength { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDexterity { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioLoseHp { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioIntangible { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioPlating { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioVigor { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioBlur { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioStrengthDown { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioLoseStrength { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioLoseDexterity { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioPoison { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioPoisonAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioEnergy { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioEnergyNextTurn { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioShiv { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioUpgradeRandom { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioUpgrade { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioUpgradeAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioExhaustOther { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioExhaustOtherRandom { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioDiscardOther { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioDiscardOtherRandom { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioExhaustAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioDiscardAll { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRandomAttack { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRandomSkill { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRandomPower { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRandomCard { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioCopy { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioCopyThis { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDeckWound { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDeckDazed { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDeckVoid { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDeckBurn { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioHandWound { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioHandBurn { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDiscardWound { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioDiscardBurn { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRetainHand { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioDoubleAttackDamageNextTurn { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioAutoPlayFromExhaust { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioRetain { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioEthereal { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioExhaust { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public bool ScenarioFinalRelease { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public int ScenarioChronicleNumber { get; set; }

    public override CardType Type => ScenarioType == (int)CardType.Attack ? CardType.Attack : CardType.Skill;

    private bool HasGameArt => Enchantment is Inky or Swift;
    private bool HasFinalRelease => ScenarioFinalRelease;
    private bool IsTalesSagaChronicle => HasGameArt && HasFinalRelease;

    public override string Title
    {
        get
        {
            var key = GetScenarioTitleKey();
            var title = key == null
                ? TitleLocString.GetFormattedText()
                : new LocString("cards", $"{Id.Entry}.title.{key}").GetFormattedText();

            if (IsTalesSagaChronicle && ScenarioChronicleNumber > 1)
            {
                title += $" {ScenarioChronicleNumber}";
            }

            if (!IsUpgraded)
            {
                return title;
            }

            return MaxUpgradeLevel > 1 ? $"{title}+{CurrentUpgradeLevel}" : title + "+";
        }
    }

    public override TargetType TargetType
    {
        get
        {
            if (HasTargetedEnemyEffect)
            {
                return TargetType.AnyEnemy;
            }

            if (HasAllEnemyEffect)
            {
                return TargetType.AllEnemies;
            }

            if (DamageMode is GameScenarioDamageMode.RandomSingle or GameScenarioDamageMode.RandomRepeat)
            {
                return TargetType.RandomEnemy;
            }

            return HasEnemyEffect ? TargetType.AnyEnemy : TargetType.Self;
        }
    }

    private GameScenarioDamageMode DamageMode => (GameScenarioDamageMode)ScenarioDamageMode;

    public bool HasPositiveSkillEffect =>
        ScenarioBlock > 0
        || ScenarioCards > 0
        || ScenarioForge > 0
        || ScenarioSummon > 0
        || ScenarioColorlessCards > 0
        || ScenarioCharge > 0
        || ScenarioShockCards > 0
        || ScenarioStrength > 0
        || ScenarioDexterity > 0
        || ScenarioIntangible > 0
        || ScenarioPlating > 0
        || ScenarioVigor > 0
        || ScenarioBlur > 0
        || ScenarioEnergy > 0
        || ScenarioEnergyNextTurn > 0
        || ScenarioShiv > 0
        || ScenarioUpgrade
        || ScenarioUpgradeRandom > 0
        || ScenarioUpgradeAll
        || ScenarioRandomAttack
        || ScenarioRandomSkill
        || ScenarioRandomPower
        || ScenarioRandomCard
        || ScenarioCopy
        || ScenarioCopyThis
        || ScenarioRetainHand
        || ScenarioDoubleAttackDamageNextTurn
        || ScenarioAutoPlayFromExhaust;

    private bool HasEnemyEffect =>
        ScenarioDamage > 0
        || ScenarioShock > 0
        || ScenarioWeak > 0
        || ScenarioVulnerable > 0
        || ScenarioStrengthDown > 0
        || ScenarioPoison > 0;

    private bool HasTargetedEnemyEffect =>
        ScenarioDamage > 0 && DamageMode is GameScenarioDamageMode.Single or GameScenarioDamageMode.Repeat
        || ScenarioShock > 0
        || ScenarioWeak > 0
        || ScenarioVulnerable > 0
        || ScenarioStrengthDown > 0
        || ScenarioPoison > 0;

    private bool HasAllEnemyEffect =>
        DamageMode == GameScenarioDamageMode.All
        || ScenarioWeakAll > 0
        || ScenarioVulnerableAll > 0
        || ScenarioPoisonAll > 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (ScenarioRetain)
            {
                yield return CardKeyword.Retain;
            }

            if (ScenarioEthereal)
            {
                yield return CardKeyword.Ethereal;
            }

            if (ScenarioExhaust)
            {
                yield return CardKeyword.Exhaust;
            }
        }
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips
    {
        get
        {
            if (ScenarioCharge > 0)
            {
                yield return ArisHoverTips.ChargePower();
            }

            if (ScenarioForge > 0)
            {
                foreach (var hoverTip in HoverTipFactory.FromForge())
                {
                    yield return hoverTip;
                }
            }

            if (ScenarioSummon > 0)
            {
                yield return HoverTipFactory.Static(StaticHoverTip.SummonDynamic, DynamicVars.Summon);
            }

            if (ScenarioShock > 0)
            {
                yield return HoverTipFactory.FromPower<ShockPower>();
            }

            if (ScenarioWeak > 0 || ScenarioWeakAll > 0)
            {
                yield return HoverTipFactory.FromPower<WeakPower>();
            }

            if (ScenarioVulnerable > 0 || ScenarioVulnerableAll > 0)
            {
                yield return HoverTipFactory.FromPower<VulnerablePower>();
            }

            if (ScenarioStrength > 0 || ScenarioStrengthDown > 0 || ScenarioLoseStrength > 0)
            {
                yield return HoverTipFactory.FromPower<StrengthPower>();
            }

            if (ScenarioDexterity > 0 || ScenarioLoseDexterity > 0)
            {
                yield return HoverTipFactory.FromPower<DexterityPower>();
            }

            if (ScenarioIntangible > 0)
            {
                yield return HoverTipFactory.FromPower<IntangiblePower>();
            }

            if (ScenarioPlating > 0)
            {
                yield return HoverTipFactory.FromPower<PlatingPower>();
            }

            if (ScenarioVigor > 0)
            {
                yield return HoverTipFactory.FromPower<VigorPower>();
            }

            if (ScenarioBlur > 0)
            {
                yield return HoverTipFactory.FromPower<BlurPower>();
            }

            if (ScenarioPoison > 0 || ScenarioPoisonAll > 0)
            {
                yield return HoverTipFactory.FromPower<PoisonPower>();
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(ScenarioDamage, ValueProp.Move),
        new DynamicVar("Hits", ScenarioDamageHits),
        new BlockVar(ScenarioBlock, ValueProp.Move),
        new CardsVar(ScenarioCards),
        new ForgeVar(ScenarioForge),
        new SummonVar(ScenarioSummon),
        new CardsVar("ColorlessCards", ScenarioColorlessCards),
        new PowerVar<ChargePower>("ChargePower", ScenarioCharge),
        new CardsVar("ShockCards", ScenarioShockCards),
        new PowerVar<ShockPower>("ShockPower", ScenarioShock),
        new PowerVar<WeakPower>("WeakPower", ScenarioWeak),
        new PowerVar<WeakPower>("WeakAllPower", ScenarioWeakAll),
        new PowerVar<VulnerablePower>("VulnerablePower", ScenarioVulnerable),
        new PowerVar<VulnerablePower>("VulnerableAllPower", ScenarioVulnerableAll),
        new PowerVar<StrengthPower>("StrengthPower", ScenarioStrength),
        new PowerVar<DexterityPower>("DexterityPower", ScenarioDexterity),
        new DynamicVar("LoseHp", ScenarioLoseHp),
        new PowerVar<IntangiblePower>("IntangiblePower", ScenarioIntangible),
        new PowerVar<PlatingPower>("PlatingPower", ScenarioPlating),
        new PowerVar<VigorPower>("VigorPower", ScenarioVigor),
        new PowerVar<BlurPower>("BlurPower", ScenarioBlur),
        new PowerVar<StrengthPower>("StrengthDownPower", ScenarioStrengthDown),
        new PowerVar<StrengthPower>("LoseStrengthPower", ScenarioLoseStrength),
        new PowerVar<DexterityPower>("LoseDexterityPower", ScenarioLoseDexterity),
        new PowerVar<PoisonPower>("PoisonPower", ScenarioPoison),
        new PowerVar<PoisonPower>("PoisonAllPower", ScenarioPoisonAll),
        new EnergyVar(ScenarioEnergy),
        new EnergyVar("EnergyNextTurnPower", ScenarioEnergyNextTurn),
        new CardsVar("ShivCards", ScenarioShiv),
        new CardsVar("UpgradeRandomCards", ScenarioUpgradeRandom),
        new CardsVar("DeckWoundCards", ScenarioDeckWound),
        new CardsVar("DeckDazedCards", ScenarioDeckDazed),
        new CardsVar("DeckVoidCards", ScenarioDeckVoid),
        new CardsVar("DeckBurnCards", ScenarioDeckBurn),
        new CardsVar("HandWoundCards", ScenarioHandWound),
        new CardsVar("HandBurnCards", ScenarioHandBurn),
        new CardsVar("DiscardWoundCards", ScenarioDiscardWound),
        new CardsVar("DiscardBurnCards", ScenarioDiscardBurn)
    ];

    public void Configure(CardType type, int cost)
    {
        ScenarioType = type == CardType.Attack ? (int)CardType.Attack : (int)CardType.Skill;
        ScenarioCost = cost;
        EnergyCost.SetCustomBaseCost(ScenarioCost);
        RefreshGeneratedValues();
    }

    public void RefreshGeneratedValues()
    {
        if (IsTalesSagaChronicle && ScenarioChronicleNumber <= 0)
        {
            ScenarioChronicleNumber = ++_chronicleCount;
        }

        RefreshScenarioKeywords();
        SetVar("Damage", ScenarioDamage);
        SetVar("Hits", ScenarioDamageHits);
        SetVar("Block", ScenarioBlock);
        SetVar("Cards", ScenarioCards);
        SetVar("Forge", ScenarioForge);
        SetVar("Summon", ScenarioSummon);
        SetVar("ColorlessCards", ScenarioColorlessCards);
        SetVar("ChargePower", ScenarioCharge);
        SetVar("ShockCards", ScenarioShockCards);
        SetVar("ShockPower", ScenarioShock);
        SetVar("WeakPower", ScenarioWeak);
        SetVar("WeakAllPower", ScenarioWeakAll);
        SetVar("VulnerablePower", ScenarioVulnerable);
        SetVar("VulnerableAllPower", ScenarioVulnerableAll);
        SetVar("StrengthPower", ScenarioStrength);
        SetVar("DexterityPower", ScenarioDexterity);
        SetVar("LoseHp", ScenarioLoseHp);
        SetVar("IntangiblePower", ScenarioIntangible);
        SetVar("PlatingPower", ScenarioPlating);
        SetVar("VigorPower", ScenarioVigor);
        SetVar("BlurPower", ScenarioBlur);
        SetVar("StrengthDownPower", ScenarioStrengthDown);
        SetVar("LoseStrengthPower", ScenarioLoseStrength);
        SetVar("LoseDexterityPower", ScenarioLoseDexterity);
        SetVar("PoisonPower", ScenarioPoison);
        SetVar("PoisonAllPower", ScenarioPoisonAll);
        SetVar("Energy", ScenarioEnergy);
        SetVar("EnergyNextTurnPower", ScenarioEnergyNextTurn);
        SetVar("ShivCards", ScenarioShiv);
        SetVar("UpgradeRandomCards", ScenarioUpgradeRandom);
        SetVar("DeckWoundCards", ScenarioDeckWound);
        SetVar("DeckDazedCards", ScenarioDeckDazed);
        SetVar("DeckVoidCards", ScenarioDeckVoid);
        SetVar("DeckBurnCards", ScenarioDeckBurn);
        SetVar("HandWoundCards", ScenarioHandWound);
        SetVar("HandBurnCards", ScenarioHandBurn);
        SetVar("DiscardWoundCards", ScenarioDiscardWound);
        SetVar("DiscardBurnCards", ScenarioDiscardBurn);
    }

    private void RefreshScenarioKeywords()
    {
        SetScenarioKeyword(CardKeyword.Retain, ScenarioRetain);
        SetScenarioKeyword(CardKeyword.Ethereal, ScenarioEthereal);
        SetScenarioKeyword(CardKeyword.Exhaust, ScenarioExhaust);
    }

    private void SetScenarioKeyword(CardKeyword keyword, bool enabled)
    {
        var localKeywords = GetKeywordsWithSources(KeywordSources.Local);
        if (enabled)
        {
            if (!localKeywords.Contains(keyword))
            {
                AddKeyword(keyword);
            }

            return;
        }

        if (localKeywords.Contains(keyword))
        {
            RemoveKeyword(keyword);
        }
    }

    private void SetVar(string name, decimal value)
    {
        if (DynamicVars.TryGetValue(name, out var dynamicVar))
        {
            dynamicVar.BaseValue = value;
        }
    }

    protected override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (ScenarioLoseHp > 0)
        {
            await CreatureCmdCompat.DamageFromCard(choiceContext, Owner.Creature, ScenarioLoseHp, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature, this, play);
        }

        await PlayDamage(choiceContext, play);

        if (ScenarioBlock > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Move, play);
        }

        if (ScenarioCards > 0)
        {
            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        }

        if (ScenarioForge > 0)
        {
            await ForgeCmd.Forge(DynamicVars.Forge.IntValue, Owner, this);
        }

        if (ScenarioSummon > 0)
        {
            await OstyCmd.Summon(choiceContext, Owner, DynamicVars.Summon.BaseValue, this);
        }

        if (ScenarioCharge > 0)
        {
            await PowerCmd.Apply<ChargePower>(choiceContext, Owner.Creature, DynamicVars["ChargePower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioStrength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioDexterity > 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, DynamicVars["DexterityPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioLoseStrength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, -DynamicVars["LoseStrengthPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioLoseDexterity > 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, -DynamicVars["LoseDexterityPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioIntangible > 0)
        {
            await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, DynamicVars["IntangiblePower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioPlating > 0)
        {
            await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature, DynamicVars["PlatingPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioVigor > 0)
        {
            await PowerCmd.Apply<VigorPower>(choiceContext, Owner.Creature, DynamicVars["VigorPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioBlur > 0)
        {
            await PowerCmd.Apply<BlurPower>(choiceContext, Owner.Creature, DynamicVars["BlurPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioEnergy > 0)
        {
            await PlayerCmd.GainEnergy(DynamicVars["Energy"].BaseValue, Owner);
        }

        if (ScenarioEnergyNextTurn > 0)
        {
            await PowerCmd.Apply<EnergyNextTurnPower>(choiceContext, Owner.Creature, DynamicVars["EnergyNextTurnPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioDoubleAttackDamageNextTurn)
        {
            await PowerCmd.Apply<ShadowStepPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        await PlayEnemyPowers(choiceContext, play);
        await PlayCardManipulation(choiceContext);
        await PlayCardGeneration();
    }

    public override async Task AfterAutoPrePlayPhaseEnteredEarly(PlayerChoiceContext choiceContext, Player player)
    {
        if (!ScenarioAutoPlayFromExhaust || player != Owner || Pile?.Type != PileType.Exhaust)
        {
            return;
        }

        await CardCmd.AutoPlay(choiceContext, this, null);
    }

    private async Task PlayDamage(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (ScenarioDamage <= 0)
        {
            return;
        }

        switch (DamageMode)
        {
            case GameScenarioDamageMode.All:
                var combatState = CombatState;
                if (combatState == null)
                {
                    return;
                }

                await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play).TargetingAllOpponents(combatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                break;
            case GameScenarioDamageMode.Repeat:
                ArgumentNullException.ThrowIfNull(play.Target);
                for (int i = 0; i < Math.Max(1, ScenarioDamageHits); i++)
                {
                    await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play).Targeting(play.Target)
                        .WithHitFx("vfx/vfx_attack_slash")
                        .Execute(choiceContext);
                }
                break;
            case GameScenarioDamageMode.RandomSingle:
                await AttackRandomEnemy(choiceContext, play);
                break;
            case GameScenarioDamageMode.RandomRepeat:
                for (int i = 0; i < Math.Max(1, ScenarioDamageHits); i++)
                {
                    await AttackRandomEnemy(choiceContext, play);
                }
                break;
            default:
                ArgumentNullException.ThrowIfNull(play.Target);
                await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play).Targeting(play.Target)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);
                break;
        }
    }

    private async Task AttackRandomEnemy(PlayerChoiceContext choiceContext, CardPlay play)
    {
        var combatState = CombatState;
        if (combatState == null)
        {
            return;
        }

        var target = Owner.RunState.Rng.CombatTargets.NextItem(combatState.HittableEnemies);
        if (target == null)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCardCompat(this, play).Targeting(target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        if (Enchantment is Inky inky && target.IsAlive)
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, target, inky.DynamicVars.Weak.BaseValue, Owner.Creature, this);
        }
    }

    private async Task PlayEnemyPowers(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (ScenarioShock > 0 || ScenarioWeak > 0 || ScenarioVulnerable > 0 || ScenarioStrengthDown > 0 || ScenarioPoison > 0)
        {
            ArgumentNullException.ThrowIfNull(play.Target);
            if (ScenarioShock > 0)
            {
                await PowerCmd.Apply<ShockPower>(choiceContext, play.Target, DynamicVars["ShockPower"].BaseValue, Owner.Creature, this);
            }

            if (ScenarioWeak > 0)
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, play.Target, DynamicVars["WeakPower"].BaseValue, Owner.Creature, this);
            }

            if (ScenarioVulnerable > 0)
            {
                await PowerCmd.Apply<VulnerablePower>(choiceContext, play.Target, DynamicVars["VulnerablePower"].BaseValue, Owner.Creature, this);
            }

            if (ScenarioStrengthDown > 0)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, play.Target, -DynamicVars["StrengthDownPower"].BaseValue, Owner.Creature, this);
            }

            if (ScenarioPoison > 0)
            {
                await PowerCmd.Apply<PoisonPower>(choiceContext, play.Target, DynamicVars["PoisonPower"].BaseValue, Owner.Creature, this);
            }
        }

        if (ScenarioWeakAll > 0)
        {
            var combatState = CombatState;
            if (combatState == null)
            {
                return;
            }

            await PowerCmd.Apply<WeakPower>(choiceContext, combatState.HittableEnemies, DynamicVars["WeakAllPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioVulnerableAll > 0)
        {
            var combatState = CombatState;
            if (combatState == null)
            {
                return;
            }

            await PowerCmd.Apply<VulnerablePower>(choiceContext, combatState.HittableEnemies, DynamicVars["VulnerableAllPower"].BaseValue, Owner.Creature, this);
        }

        if (ScenarioPoisonAll > 0)
        {
            var combatState = CombatState;
            if (combatState == null)
            {
                return;
            }

            await PowerCmd.Apply<PoisonPower>(choiceContext, combatState.HittableEnemies, DynamicVars["PoisonAllPower"].BaseValue, Owner.Creature, this);
        }
    }

    private async Task PlayCardManipulation(PlayerChoiceContext choiceContext)
    {
        if (ScenarioExhaustOther)
        {
            var card = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 1), null, this)).FirstOrDefault();
            if (card != null)
            {
                await CardCmdCompat.Exhaust(choiceContext, card);
            }
        }

        if (ScenarioExhaustOtherRandom)
        {
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(PileType.Hand.GetPile(Owner).Cards.Where(card => card != this));
            if (card != null)
            {
                await CardCmdCompat.Exhaust(choiceContext, card);
            }
        }

        if (ScenarioDiscardOther)
        {
            var cards = await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), card => card != this, this);
            await CardCmd.Discard(choiceContext, cards);
        }

        if (ScenarioDiscardOtherRandom)
        {
            var card = Owner.RunState.Rng.CombatCardSelection.NextItem(PileType.Hand.GetPile(Owner).Cards.Where(card => card != this));
            if (card != null)
            {
                await CardCmd.Discard(choiceContext, card);
            }
        }

        if (ScenarioExhaustAll)
        {
            var cards = PileType.Hand.GetPile(Owner).Cards.Where(card => card != this).ToList();
            foreach (var card in cards)
            {
                await CardCmdCompat.Exhaust(choiceContext, card);
            }
        }

        if (ScenarioDiscardAll)
        {
            var cards = PileType.Hand.GetPile(Owner).Cards.Where(card => card != this).ToList();
            await CardCmd.Discard(choiceContext, cards);
        }

        if (ScenarioUpgrade)
        {
            var card = await CardSelectCmd.FromHandForUpgrade(choiceContext, Owner, this);
            if (card != null)
            {
                CardCmd.Upgrade(card);
            }
        }

        if (ScenarioUpgradeRandom > 0)
        {
            var cards = PileType.Hand.GetPile(Owner).Cards
                .Where(card => card != this && card.IsUpgradable)
                .TakeRandom(ScenarioUpgradeRandom, Owner.RunState.Rng.CombatCardSelection);
            CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
        }

        if (ScenarioUpgradeAll)
        {
            var cards = PileType.Hand.GetPile(Owner).Cards
                .Where(card => card != this && card.IsUpgradable)
                .ToList();
            CardCmd.Upgrade(cards, CardPreviewStyle.HorizontalLayout);
        }

        if (ScenarioCopy)
        {
            var card = (await CardSelectCmd.FromHand(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), card => card != this, this)).FirstOrDefault();
            if (card != null)
            {
                await CardPileCmd.AddGeneratedCardToCombat(card.CreateClone(), PileType.Hand, Owner);
            }
        }

        if (ScenarioCopyThis)
        {
            await CardPileCmd.AddGeneratedCardToCombat(CreateClone(), PileType.Discard, Owner);
        }
    }

    private async Task PlayCardGeneration()
    {
        if (ScenarioShiv > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Shock>(ScenarioShiv), PileType.Hand, Owner);
        }

        if (ScenarioShockCards > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Shock>(ScenarioShockCards), PileType.Hand, Owner);
        }

        if (ScenarioDeckWound > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Wound>(ScenarioDeckWound), PileType.Draw, Owner, CardPilePosition.Random);
        }

        if (ScenarioDeckDazed > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Dazed>(ScenarioDeckDazed), PileType.Draw, Owner, CardPilePosition.Random);
        }

        if (ScenarioDeckVoid > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Void>(ScenarioDeckVoid), PileType.Draw, Owner, CardPilePosition.Random);
        }

        if (ScenarioDeckBurn > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Burn>(ScenarioDeckBurn), PileType.Draw, Owner, CardPilePosition.Random);
        }

        if (ScenarioHandWound > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Wound>(ScenarioHandWound), PileType.Hand, Owner);
        }

        if (ScenarioHandBurn > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Burn>(ScenarioHandBurn), PileType.Hand, Owner);
        }

        if (ScenarioDiscardWound > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Wound>(ScenarioDiscardWound), PileType.Discard, Owner);
        }

        if (ScenarioDiscardBurn > 0)
        {
            await CardPileCmd.AddGeneratedCardsToCombat(CreateCards<Burn>(ScenarioDiscardBurn), PileType.Discard, Owner);
        }

        if (ScenarioRandomAttack)
        {
            await AddRandomCard(card => card.Type == CardType.Attack);
        }

        if (ScenarioRandomSkill)
        {
            await AddRandomCard(card => card.Type == CardType.Skill);
        }

        if (ScenarioRandomPower)
        {
            await AddRandomCard(card => card.Type == CardType.Power);
        }

        if (ScenarioRandomCard)
        {
            await AddRandomCard(_ => true);
        }

        if (ScenarioColorlessCards > 0)
        {
            var cards = CardFactory.GetForCombat(Owner,
                ModelDb.CardPool<ColorlessCardPool>().GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint),
                ScenarioColorlessCards,
                Owner.RunState.Rng.CombatCardGeneration).ToList();
            await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, Owner);
        }

        if (ScenarioRetainHand)
        {
            await PowerCmd.Apply<RetainHandPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 1m, Owner.Creature, this);
        }
    }

    private IEnumerable<CardModel> CreateCards<T>(int count) where T : CardModel
    {
        var combatState = CombatState;
        if (combatState == null)
        {
            yield break;
        }

        for (int i = 0; i < count; i++)
        {
            yield return combatState.CreateCard<T>(Owner);
        }
    }

    private async Task AddRandomCard(Func<CardModel, bool> filter)
    {
        var card = CardFactory.GetDistinctForCombat(Owner,
            Owner.Character.CardPool.GetUnlockedCards(Owner.UnlockState, Owner.RunState.CardMultiplayerConstraint)
                .Where(card => card.CanBeGeneratedInCombat && filter(card)),
            1,
            Owner.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (card == null)
        {
            return;
        }

        card.SetToFreeThisTurn();
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        if (ScenarioCost > 0)
        {
            ScenarioCost--;
            EnergyCost.UpgradeBy(-1);
        }
        else
        {
            ScenarioRetain = true;
        }

        RefreshGeneratedValues();
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("GeneratedDescription", BuildGeneratedDescription());
    }

    private string BuildGeneratedDescription()
    {
        List<string> lines = [];

        AddLine(lines, ScenarioLoseHp > 0, "loseHp");
        AddDamageLine(lines);
        AddLine(lines, ScenarioBlock > 0, "block");
        AddLine(lines, ScenarioCards > 0, "cards");
        AddLine(lines, ScenarioForge > 0, "forge");
        AddLine(lines, ScenarioSummon > 0, "summon");
        AddLine(lines, ScenarioColorlessCards > 0, "colorlessCards");
        AddLine(lines, ScenarioCharge > 0, "charge");
        AddLine(lines, ScenarioShockCards > 0, "shockCards");
        AddLine(lines, ScenarioShock > 0, "shock");
        AddLine(lines, ScenarioWeak > 0, "weak");
        AddLine(lines, ScenarioWeakAll > 0, "weakAll");
        AddLine(lines, ScenarioVulnerable > 0, "vulnerable");
        AddLine(lines, ScenarioVulnerableAll > 0, "vulnerableAll");
        AddLine(lines, ScenarioStrength > 0, "strength");
        AddLine(lines, ScenarioDexterity > 0, "dexterity");
        AddLine(lines, ScenarioIntangible > 0, "intangible");
        AddLine(lines, ScenarioPlating > 0, "plating");
        AddLine(lines, ScenarioVigor > 0, "vigor");
        AddLine(lines, ScenarioBlur > 0, "blur");
        AddLine(lines, ScenarioStrengthDown > 0, "strengthDown");
        AddLine(lines, ScenarioLoseStrength > 0, "loseStrength");
        AddLine(lines, ScenarioLoseDexterity > 0, "loseDexterity");
        AddLine(lines, ScenarioPoison > 0, "poison");
        AddLine(lines, ScenarioPoisonAll > 0, "poisonAll");
        AddLine(lines, ScenarioEnergy > 0, "energy");
        AddLine(lines, ScenarioEnergyNextTurn > 0, "energyNextTurn");
        AddLine(lines, ScenarioExhaustOther, "exhaustOther");
        AddLine(lines, ScenarioExhaustOtherRandom, "exhaustOtherRandom");
        AddLine(lines, ScenarioDiscardOther, "discardOther");
        AddLine(lines, ScenarioDiscardOtherRandom, "discardOtherRandom");
        AddLine(lines, ScenarioExhaustAll, "exhaustAll");
        AddLine(lines, ScenarioDiscardAll, "discardAll");
        AddLine(lines, ScenarioRandomAttack, "randomAttack");
        AddLine(lines, ScenarioRandomSkill, "randomSkill");
        AddLine(lines, ScenarioRandomPower, "randomPower");
        AddLine(lines, ScenarioRandomCard, "randomCard");
        AddLine(lines, ScenarioShiv > 0, "shiv");
        AddLine(lines, ScenarioUpgrade, "upgrade");
        AddLine(lines, ScenarioUpgradeRandom > 0, "upgradeRandom");
        AddLine(lines, ScenarioUpgradeAll, "upgradeAll");
        AddLine(lines, ScenarioCopy, "copy");
        AddLine(lines, ScenarioCopyThis, "copyThis");
        AddLine(lines, ScenarioDeckWound > 0, "deckWound");
        AddLine(lines, ScenarioDeckDazed > 0, "deckDazed");
        AddLine(lines, ScenarioDeckVoid > 0, "deckVoid");
        AddLine(lines, ScenarioDeckBurn > 0, "deckBurn");
        AddLine(lines, ScenarioHandWound > 0, "handWound");
        AddLine(lines, ScenarioHandBurn > 0, "handBurn");
        AddLine(lines, ScenarioDiscardWound > 0, "discardWound");
        AddLine(lines, ScenarioDiscardBurn > 0, "discardBurn");
        AddLine(lines, ScenarioRetainHand, "retainHand");
        AddLine(lines, ScenarioDoubleAttackDamageNextTurn, "doubleAttackDamageNextTurn");
        AddLine(lines, ScenarioAutoPlayFromExhaust, "autoPlayFromExhaust");

        if (lines.Count == 0)
        {
            return GetScenarioDescriptionPart("empty").GetFormattedText();
        }

        return string.Join('\n', lines);
    }

    private void AddDamageLine(List<string> lines)
    {
        if (ScenarioDamage <= 0)
        {
            return;
        }

        var key = DamageMode switch
        {
            GameScenarioDamageMode.All => "damageAll",
            GameScenarioDamageMode.Repeat => "damageRepeat",
            GameScenarioDamageMode.RandomSingle => "damageRandom",
            GameScenarioDamageMode.RandomRepeat => "damageRandomRepeat",
            _ => "damage"
        };
        lines.Add(GetScenarioDescriptionPart(key).GetFormattedText());
    }

    private void AddLine(List<string> lines, bool include, string key)
    {
        if (!include)
        {
            return;
        }

        lines.Add(GetScenarioDescriptionPart(key).GetFormattedText());
    }

    private LocString GetScenarioDescriptionPart(string key)
    {
        var line = new LocString("cards", $"{Id.Entry}.description.{key}");
        DynamicVars.AddTo(line);
        base.AddExtraArgsToDescription(line);

        string energyPrefix = EnergyIconHelper.GetPrefix(this);
        line.Add("energyPrefix", energyPrefix);
        foreach (object variable in line.Variables.Values)
        {
            if (variable is EnergyVar energyVar)
            {
                energyVar.ColorPrefix = energyPrefix;
            }
        }

        return line;
    }

    private string? GetScenarioTitleKey()
    {
        if (IsTalesSagaChronicle)
        {
            return "talesSagaChronicle";
        }

        if (HasGameArt)
        {
            return "prototype";
        }

        return HasFinalRelease ? "betaTest" : null;
    }

    private string ScenarioPortraitName
    {
        get
        {
            if (IsTalesSagaChronicle)
            {
                return "game_scenario_tales_saga_chronicle";
            }

            if (HasGameArt)
            {
                return "game_scenario_prototype";
            }

            if (HasFinalRelease)
            {
                return "game_scenario_beta_test";
            }

            return "game_scenario";
        }
    }

    public override string CustomPortraitPath => $"{ScenarioPortraitName}_p.png".CardImagePath();

    public override string PortraitPath => $"{ScenarioPortraitName}.png".CardImagePath();
}
