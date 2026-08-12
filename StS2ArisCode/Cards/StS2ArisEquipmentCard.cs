using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using StS2Aris.StS2ArisCode.CardModels;
using StS2Aris.StS2ArisCode.Hooks;
using StS2Aris.StS2ArisCode.Mechanics;
using StS2Aris.StS2ArisCode.Powers;

namespace StS2Aris.StS2ArisCode.Cards;

public abstract class StS2ArisEquipmentCard(int cost, CardType type, CardRarity rarity, TargetType targetType)
    : StS2ArisCard(cost, type, rarity, targetType), IArisEquipmentCard
{
    private bool _repeatClassChangeForCurrentPlaySeries;

    public abstract ArisJobPower CreateJobPower();

    protected static T MakeJobPower<T>() where T : ArisJobPower
    {
        return (T)ModelDb.Power<T>().ToMutable();
    }

    protected sealed override async Task OnArisPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.PlayIndex > 0)
        {
            if (_repeatClassChangeForCurrentPlaySeries &&
                ArisEquipment.GetCurrentJob(Owner) is { } currentJob &&
                ReferenceEquals(currentJob.EquipmentCard, this))
            {
                await TriggerClassChange(choiceContext, play, currentJob);
            }

            if (play.IsLastInSeries)
            {
                _repeatClassChangeForCurrentPlaySeries = false;
            }

            return;
        }

        var nextJob = CreateJobPower();
        var changesJob = ArisEquipment.ShouldTriggerClassChange(Owner, nextJob);
        _repeatClassChangeForCurrentPlaySeries = changesJob && play.PlayCount > 1;
        if (changesJob)
        {
            if (!await TriggerClassChange(choiceContext, play, nextJob))
            {
                _repeatClassChangeForCurrentPlaySeries = false;
                return;
            }
        }

        await ArisEquipment.Equip(choiceContext, this, nextJob);

        if (play.IsLastInSeries)
        {
            _repeatClassChangeForCurrentPlaySeries = false;
        }
    }

    private async Task<bool> TriggerClassChange(
        PlayerChoiceContext choiceContext,
        CardPlay play,
        ArisJobPower job)
    {
        job.EquipmentCard = this;
        await job.OnClassChange(choiceContext, play);
        if (Owner.Creature.IsDead)
        {
            return false;
        }

        await ArisHook.OnClassChanged(choiceContext, Owner, this);
        return !Owner.Creature.IsDead;
    }

}
