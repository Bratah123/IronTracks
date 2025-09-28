using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TPCI.Rainier.Match.Cards;

namespace PTCGLDeckTracker.CardCollection
{
    internal class PrizeCards : CardCollection
    {
        public Dictionary<string, Card> KnownPrizeCards { get; set; } = new Dictionary<string, Card>();

        // These are the prize cards users have already taken
        public Dictionary<string, Card> RemovedPrizedCards { get; set; } = new Dictionary<string, Card>();


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
            // Ignore if the card is unknown
            if (string.IsNullOrEmpty(cardAdded.entityID) || cardAdded.entityID.Equals("PRIVATE"))
            {
                return;
            }
            // If the card is seen by the player but not already marked as seen (Hisuian Heavy Ball, Peonia) internally
            // Add that to KnownPrizeCards
            if (!KnownPrizeCards.ContainsKey(cardAdded.cardSourceID))
            {
                var card = new Card(cardAdded.cardSourceID)
                {
                    quantity = 1,
                    englishName = Card.GetEnglishNameFromCard3DName(cardAdded.name)
                };
                KnownPrizeCards.Add(cardAdded.cardSourceID, card);
            }
            else
            {
                KnownPrizeCards[cardAdded.cardSourceID].quantity++;
            }
            // Check to see if it exists in RemovedPrizeCards and update dictionary accordingly
            if (RemovedPrizedCards.ContainsKey(cardAdded.cardSourceID))
            {
                RemovedPrizedCards[cardAdded.cardSourceID].quantity--;
            }
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            base.OnCardRemoved(cardRemoved);
            Melon<IronTracks>.Logger.Msg("Added Card " + cardRemoved.name + " into hand.");
            // Ignore if the card is unknown (this technically shouldn't be possible AFAIK)
            if (string.IsNullOrEmpty(cardRemoved.entityID) || cardRemoved.entityID.Equals("PRIVATE"))
            {
                return;
            }
            if (KnownPrizeCards.ContainsKey(cardRemoved.cardSourceID))
            {
                KnownPrizeCards[cardRemoved.cardSourceID].quantity--;
            }
            else
            {
                // If KnownPrizeCards does not contain the card removed
                // Then the user took prize cards without ever deck searching
                if (RemovedPrizedCards.ContainsKey(cardRemoved.cardSourceID))
                {
                    RemovedPrizedCards[cardRemoved.cardSourceID].quantity++;
                }
                else
                {
                    var card = new Card(cardRemoved.cardSourceID)
                    {
                        quantity = 1,
                        englishName = Card.GetEnglishNameFromCard3DName(cardRemoved.name)
                    };
                    RemovedPrizedCards.Add(cardRemoved.cardSourceID, card);
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
                total += kvp.Value.quantity;
            }
            return total;
        }

        public int GetKnownPrizeCardsCount()
        {
            int total = 0;
            foreach (var kvp in KnownPrizeCards)
            {
                total += kvp.Value.quantity;
            }
            return total;
        }

        public string PrizeCardStringForRender()
        {
            var renderString = "";
            if (GetKnownPrizeCardsCount() == 0)
            {
                return renderString;
            }
            foreach (var kvp in KnownPrizeCards)
            {
                var card = kvp.Value;
                if (card.quantity == 0)
                {
                    continue;
                }
                renderString += card.quantity + " " + card + "\n";
            }
            renderString += "\nTotal Prize Cards: " + _cardCount;
            return renderString;
        }
    }
}
