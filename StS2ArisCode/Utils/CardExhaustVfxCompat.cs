using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace StS2Aris.StS2ArisCode.Utils;

internal static class CardExhaustVfxCompat
{
    private static readonly Type? ModernVfxType =
        AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Vfx.Cards.NCardExhaustVfx");
    private static readonly Type? LegacyVfxType =
        AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Vfx.Cards.NExhaustVfx");

    public static void Play(CardModel card)
    {
        var room = NCombatRoom.Instance;
        if (!LocalContext.IsMe(card.Owner) || room == null || NCard.FindOnTable(card) != null)
        {
            return;
        }

        var cardNode = NCard.Create(card);
        if (cardNode == null)
        {
            return;
        }

        room.CombatVfxContainer.AddChildSafely(cardNode);
        cardNode.GlobalPosition = PileType.Play.GetTargetPosition(cardNode);
        cardNode.UpdateVisuals(PileType.Play, CardPreviewMode.Normal);

        if (TryPlayModern(room, cardNode) || TryPlayLegacy(room, cardNode))
        {
            return;
        }

        cardNode.QueueFreeSafely();
    }

    private static bool TryPlayModern(NCombatRoom room, NCard cardNode)
    {
        var create = ModernVfxType == null
            ? null
            : AccessTools.DeclaredMethod(ModernVfxType, "Create", [typeof(NCard)]);
        var playAnimation = ModernVfxType == null
            ? null
            : AccessTools.DeclaredMethod(ModernVfxType, "PlayAnimation");
        if (create == null || playAnimation == null)
        {
            return false;
        }

        Node? vfx = null;
        try
        {
            vfx = create.Invoke(null, [cardNode]) as Node;
            if (vfx == null)
            {
                return false;
            }

            room.Ui.AddChildSafely(vfx);
            NDebugAudioManager.Instance?.Play("card_exhaust.mp3");
            if (playAnimation.Invoke(vfx, null) is Task task)
            {
                TaskHelper.RunSafely(task);
            }

            return true;
        }
        catch
        {
            vfx?.QueueFreeSafely();
            return false;
        }
    }

    private static bool TryPlayLegacy(NCombatRoom room, NCard cardNode)
    {
        var create = LegacyVfxType == null
            ? null
            : AccessTools.DeclaredMethod(LegacyVfxType, "Create", [typeof(NCard)]);
        if (create == null)
        {
            return false;
        }

        try
        {
            if (create.Invoke(null, [cardNode]) is not Node vfx)
            {
                return false;
            }

            room.Ui.AddChildSafely(vfx);
            var tween = cardNode.CreateTween();
            tween.TweenProperty(cardNode, "modulate", StsColors.exhaustGray, 0.3f);
            tween.TweenCallback(Callable.From(cardNode.QueueFreeSafely));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
