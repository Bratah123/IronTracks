using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCI.Rainier.Match.Cards;
using UnityEngine;

namespace PTCGLDeckTracker.CardCollection
{
    internal class PrizeCards : CardCollection
    {
        public Dictionary<string, TrackedCard> KnownPrizeCards { get; set; } = new Dictionary<string, TrackedCard>();
        public Dictionary<string, TrackedCard> RemovedPrizedCards { get; set; } = new Dictionary<string, TrackedCard>();

        override public void Clear()
        {
            base.Clear();
            KnownPrizeCards.Clear();
            RemovedPrizedCards.Clear();
        }

        public override void OnCardAdded(Card3D cardAdded)
        {
            base.OnCardAdded(cardAdded);
            Melon<IronTracks>.Logger.Msg("Added Card " + cardAdded.name + " into Prize Cards.");
            if (string.IsNullOrEmpty(cardAdded.entityID) || cardAdded.entityID.Equals("PRIVATE"))
            {
                return;
            }

            if (!KnownPrizeCards.ContainsKey(cardAdded.cardSourceID))
            {
                var card = new Card(cardAdded.cardSourceID)
                {
                    quantity = 1,
                    englishName = Card.GetEnglishNameFromCard3DName(cardAdded.name)
                };
                var trackedCard = new TrackedCard(card);
                trackedCard.highlightState = HighlightState.Added;
                trackedCard.highlightEndTime = Time.time + 2.0f;
                KnownPrizeCards.Add(cardAdded.cardSourceID, trackedCard);
            }
            else
            {
                var trackedCard = KnownPrizeCards[cardAdded.cardSourceID];
                trackedCard.card.quantity++;
                trackedCard.highlightState = HighlightState.Added;
                trackedCard.highlightEndTime = Time.time + 2.0f;
            }

            if (RemovedPrizedCards.ContainsKey(cardAdded.cardSourceID))
            {
                RemovedPrizedCards[cardAdded.cardSourceID].card.quantity--;
            }
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            base.OnCardRemoved(cardRemoved);
            Melon<IronTracks>.Logger.Msg("Added Card " + cardRemoved.name + " into hand.");
            if (string.IsNullOrEmpty(cardRemoved.entityID) || cardRemoved.entityID.Equals("PRIVATE"))
            {
                return;
            }
            if (KnownPrizeCards.ContainsKey(cardRemoved.cardSourceID))
            {
                var trackedCard = KnownPrizeCards[cardRemoved.cardSourceID];
                trackedCard.card.quantity--;
                trackedCard.highlightState = HighlightState.Removed;
                trackedCard.highlightEndTime = Time.time + 2.0f;
            }
            else
            {
                if (RemovedPrizedCards.ContainsKey(cardRemoved.cardSourceID))
                {
                    RemovedPrizedCards[cardRemoved.cardSourceID].card.quantity++;
                }
                else
                {
                    var card = new Card(cardRemoved.cardSourceID)
                    {
                        quantity = 1,
                        englishName = Card.GetEnglishNameFromCard3DName(cardRemoved.name)
                    };
                    RemovedPrizedCards.Add(cardRemoved.cardSourceID, new TrackedCard(card));
                }
            }
        }

        public int GetPrizeCount()
        {
            return _cardCount;
        }

        public int GetRemovedPrizeCardsCount()
        {
            int total = 0;
            foreach (var kvp in RemovedPrizedCards)
            {
                total += kvp.Value.card.quantity;
            }
            return total;
        }

        public int GetKnownPrizeCardsCount()
        {
            int total = 0;
            foreach (var kvp in KnownPrizeCards)
            {
                total += kvp.Value.card.quantity;
            }
            return total;
        }

        public List<TrackedCard> GetCardsForRender()
        {
            var cards = new List<TrackedCard>();
            foreach (var kvp in KnownPrizeCards)
            {
                var card = kvp.Value;
                if (card.card.quantity == 0)
                {
                    continue;
                }
                cards.Add(card);
            }
            return cards;
        }
    }
}