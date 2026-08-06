using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using StS2Aris.StS2ArisCode.CardModels;
using StS2Aris.StS2ArisCode.Character;
using StS2Aris.StS2ArisCode.Config;
using StS2Aris.StS2ArisCode.Events;
using StS2Aris.StS2ArisCode.Keywords;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Cards;

[Pool(typeof(StS2ArisCardPool))]
public class SuperNova() : StS2ArisEquipmentCard(1, CardType.Attack, CardRarity.Basic, TargetType.AllEnemies), ITranscendenceCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromKeyword(ArisKeywords.Equipment),
        HoverTipFactory.FromKeyword(ArisKeywords.ClassChange),
        HoverTipFactory.FromKeyword(ArisKeywords.Job),
        HoverTipFactory.FromPower<StrengthPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new PowerVar<StrengthPower>(3m)
    ];

    public override ArisJobPower CreateJobPower()
    {
        return MakeJobPower<JobAoePower>();
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
    
    public CardModel GetTranscendenceTransformedCard()
    {
        return ModelDb.Card<AtrahasisSuperNova>();
    }

    public override EventModel ModifyNextEvent(EventModel currentEvent)
    {
        var runState = Owner.RunState;
        if (runState.Players.Count > 1 || !ArisModConfig.ForceClassAltarFirstEvent)
        {
            return currentEvent;
        }

        bool hasVisitedNormalEvent = runState.MapPointHistory
            .SelectMany(actHistory => actHistory)
            .Any(entry => entry.MapPointType != MapPointType.Ancient && entry.HasRoomOfType(RoomType.Event));
        if (hasVisitedNormalEvent)
        {
            return currentEvent;
        }

        ClassAltar classAltar = ModelDb.Event<ClassAltar>();
        return classAltar.IsAllowed(runState) ? classAltar : currentEvent;
    }
}
