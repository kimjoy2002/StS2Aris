using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using StS2Aris.StS2ArisCode.Cards;
using StS2Aris.StS2ArisCode.Extensions;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Relics;

namespace StS2Aris.StS2ArisCode.Character;

public class StS2Aris : PlaceholderCharacterModel
{
    public const string ModId = "StS2Aris";
    public const string CharacterId = "StS2Aris";

    public static readonly Color Color = new("0a0ac8");

    public override Color NameColor => Color;
    public override Color MapDrawingColor => Color;
    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<ArisStrike>(),
        ModelDb.Card<ArisStrike>(),
        ModelDb.Card<ArisStrike>(),
        ModelDb.Card<ArisStrike>(),
        ModelDb.Card<ArisDefend>(),
        ModelDb.Card<ArisDefend>(),
        ModelDb.Card<ArisDefend>(),
        ModelDb.Card<ArisDefend>(),
        ModelDb.Card<SuperNova>(),
        ModelDb.Card<EnergyCharge>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<ArisBaseRelic>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<StS2ArisCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<StS2ArisRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<StS2ArisPotionPool>();
    protected override IEnumerable<string> ExtraAssetPaths =>
    [
        "aris_charge.png".CharacterUiPath(),
        "aris_charge_layer_2.png".CharacterUiPath(),
        "aris_charge_layer_3.png".CharacterUiPath(),
        "aris_charge_layer_4.png".CharacterUiPath(),
        "aris_charge_layer_5.png".CharacterUiPath(),
        "aris_charge_layer_6.png".CharacterUiPath(),
        "res://StS2Aris/etc/kreon_bold_shared.tres"
    ];

    public override RelicIconData CustomYummyCookie => new(
        "yummy_cookie_aris.png".BigRelicImagePath(),
        "yummy_cookie_aris.png".RelicImagePath(),
        "yummy_cookie_aris_outline.png".RelicImagePath()
    );

    
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;
    public override string CharacterSelectSfx => "Aris_light.mp3".SfxPath();

	public override string CustomIconPath => "res://StS2Aris/scenes/icon_aris.tscn";
    public override string CustomIconTexturePath => "character_icon_aris.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_aris.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_aris_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_aris.png".CharacterUiPath();
    public override string CustomVisualPath => "res://StS2Aris/scenes/char_aris.tscn";
    public override string CustomCharacterSelectBg => "res://StS2Aris/scenes/char_select_bg_aris.tscn";
    public override string CustomRestSiteAnimPath => "res://StS2Aris/scenes/aris_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://StS2Aris/scenes/aris_merchant.tscn";

    public override NCreatureVisuals CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualPath);
    }
    
    
    public override string CustomArmPointingTexturePath =>
        "multiplayer_hand_aris_point.png".CharacterUiPath();

    public override string CustomArmRockTexturePath =>
        "multiplayer_hand_aris_rock.png".CharacterUiPath();

    public override string CustomArmPaperTexturePath =>
        "multiplayer_hand_aris_paper.png".CharacterUiPath();

    public override string CustomArmScissorsTexturePath =>
        "multiplayer_hand_aris_scissor.png".CharacterUiPath();

    

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("idle_loop", true);
        var attack = new AnimState("attack", false) { NextState = idle };
        var cast = new AnimState("cast", false) { NextState = idle };
        var hit = new AnimState("hurt", false) { NextState = idle };
        var dead = new AnimState("die", false);

        var animator = new CreatureAnimator(idle, controller);

        MegaCrit.Sts2.Core.Entities.Creatures.Creature? GetCreature()
        {
            var current = controller.BoundObject as Node;
            while (current != null)
            {
                if (current is NCreature nCreature)
                {
                    return nCreature.Entity;
                }

                current = current.GetParent();
            }

            return null;
        }

        string? CurrentJobSuffix()
        {
            var player = GetCreature()?.Player;
            return player == null ? null : ArisEquipment.GetCurrentAnimationSuffix(player);
        }

        var jobIdleStates = new Dictionary<string, AnimState>();

        void AddJobIdleState(string suffix)
        {
            var state = new AnimState($"idle_loop_{suffix}", true);
            jobIdleStates[suffix] = state;
            animator.AddAnyState("Idle", state, () => CurrentJobSuffix() == suffix);
        }

        void AddJobActionState(string trigger, string animationPrefix, string suffix)
        {
            var nextIdle = jobIdleStates.TryGetValue(suffix, out var jobIdle) ? jobIdle : idle;
            var state = new AnimState($"{animationPrefix}_{suffix}", false) { NextState = nextIdle };
            animator.AddAnyState(trigger, state, () => CurrentJobSuffix() == suffix);
        }

        foreach (var suffix in new[] { "AOEDPS", "Newby", "Rogue", "Wizard", "Warrior", "Idol", "Hero", "Maid", "Necromancer", "Kei", "Defect", "Regent", "Hoshino", "Rabbit"})
        {
            AddJobIdleState(suffix);
            AddJobActionState("Attack", "attack", suffix);
            AddJobActionState("Cast", "cast", suffix);
            AddJobActionState("Hit", "hurt", suffix);
        }

        AddJobActionState("Attack2", "attack2", "AOEDPS");

        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Hit", hit);
        animator.AddAnyState("Dead", dead);

        return animator;
    }
}
