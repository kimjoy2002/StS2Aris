using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;

namespace StS2Aris.StS2ArisCode.Utils;

public static class CardPlayCompat
{
    private static readonly PropertyInfo? PlayerProperty = typeof(CardPlay).GetProperty("Player");

    public static Player GetPlayer(this CardPlay cardPlay)
    {
        return PlayerProperty?.GetValue(cardPlay) as Player ?? cardPlay.Card.Owner;
    }

    public static CardPlay CreateForEffect(CardModel card) => new EffectCardPlay(card);

    private sealed class EffectCardPlay : CardPlay
    {
        // Beta's required Player is initialized through reflection below.
#pragma warning disable CS8618
        [SetsRequiredMembers]
        public EffectCardPlay(CardModel card)
        {
            Card = card;
            Target = null;
            ResultPile = PileType.None;
            Resources = default;
            IsAutoPlay = true;
            PlayIndex = 0;
            PlayCount = 1;
            // Only beta has a separate Player property; main uses Card.Owner.
            PlayerProperty?.SetValue(this, card.Owner);
        }
#pragma warning restore CS8618
    }
}
