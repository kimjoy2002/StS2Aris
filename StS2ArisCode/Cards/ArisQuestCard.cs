using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using StS2Aris.StS2ArisCode.Utils;
using StS2Aris.StS2ArisCode.Mechanics;

namespace StS2Aris.StS2ArisCode.Cards;

public abstract class ArisQuestCard<TReward>(int cost, CardType type, CardRarity rarity, TargetType target)
    : StS2ArisCard(cost, type, rarity, target), IArisQuestProgressCard, IArisReplicaReward where TReward : CardModel
{
    private int _questRemaining;

    public override bool IsArisQuest => true;

    public abstract int QuestGoal { get; }

    public virtual int QuestProgressCurrent => int.Clamp(QuestGoal - QuestRemaining, 0, QuestProgressGoal);

    public virtual int QuestProgressGoal => QuestGoal;

    protected virtual bool RewardInheritsUpgrade => true;

    [SavedProperty]
    public int QuestRemaining
    {
        get => _questRemaining;
        set
        {
            AssertMutable();
            _questRemaining = value;
        }
    }

    public override void AfterCreated()
    {
        base.AfterCreated();
        EnsureQuestInitialized();
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        EnsureQuestInitialized();
    }

    protected async Task AdvanceQuest(int amount = 1)
    {
        if (!CanAdvanceQuest())
        {
            return;
        }

        EnsureQuestInitialized();
        QuestRemaining = int.Max(0, QuestRemaining - amount);
        if (QuestRemaining == 0)
        {
            await CompleteQuest();
        }
    }

    protected async Task CompleteQuestIf(bool condition)
    {
        if (!condition || !CanAdvanceQuest())
        {
            return;
        }

        await CompleteQuest();
    }

    protected void ResetQuestProgress()
    {
        if (CanAdvanceQuest())
        {
            QuestRemaining = QuestGoal;
        }
    }

    protected virtual Task BeforeQuestComplete()
    {
        return Task.CompletedTask;
    }

    protected virtual CardModel CreateRewardCard()
    {
        TReward reward = Owner.RunState.CreateCard<TReward>(Owner);
        if (RewardInheritsUpgrade && IsUpgraded)
        {
            reward.UpgradeInternal();
            reward.FinalizeUpgradeInternal();
        }

        CopyEnchantmentToReward(reward);
        return reward;
    }

    protected void CopyEnchantmentToReward(CardModel reward)
    {
        ArisQuestUtils.TryCopyEnchantment(this, reward);
    }

    public virtual async Task<CardPileAddResult?> ApplyReplicaReward(bool forceUpgrade)
    {
        return await CardPileCmd.Add(CreateReplicaRewardCard(forceUpgrade), PileType.Deck);
    }

    protected CardModel CreateReplicaRewardCard(bool forceUpgrade)
    {
        CardModel reward = CreateRewardCard();
        if (forceUpgrade && !reward.IsUpgraded && reward.IsUpgradable)
        {
            reward.UpgradeInternal();
            reward.FinalizeUpgradeInternal();
        }

        return reward;
    }

    protected decimal GetReplicaRewardValue(string dynamicVarName, bool forceUpgrade)
    {
        if (!forceUpgrade || IsUpgraded || !IsUpgradable)
        {
            return DynamicVars[dynamicVarName].BaseValue;
        }

        CardModel upgradedCopy = (CardModel)MutableClone();
        upgradedCopy.UpgradeInternal();
        return upgradedCopy.DynamicVars[dynamicVarName].BaseValue;
    }

    protected async Task CompleteQuest()
    {
        if (!CanAdvanceQuest())
        {
            return;
        }

        await BeforeQuestComplete();
        PlayerCmd.CompleteQuest(this);
        await ApplyQuestReward();
        if (this is not BingoBoard)
        {
            await ArisQuestProgress.NotifyCompleted(Owner, this);
        }
    }

    protected virtual async Task ApplyQuestReward()
    {
        await CardCmd.Transform(this, CreateRewardCard());
    }

    private void EnsureQuestInitialized()
    {
        if (QuestRemaining <= 0)
        {
            QuestRemaining = QuestGoal;
        }
    }

    private bool CanAdvanceQuest()
    {
        return Owner != null && Pile?.Type == PileType.Deck && !HasBeenRemovedFromState;
    }

}
