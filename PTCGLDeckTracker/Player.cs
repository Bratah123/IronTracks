using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using PTCGLDeckTracker.CardCollection;
using static ContentItemCellRow;
using static MelonLoader.MelonLogger;

namespace PTCGLDeckTracker
{
    internal class Player
    {
        public string username { get; set; }

        // Card Collections
        public Deck deck { get; set; }
        public PrizeCards prizeCards { get; set; }
        public DiscardPile discardPile { get; set; }
        public Hand hand { get; set; }

        public Player()
        {
            this.deck = new Deck("playerOne");
            this.prizeCards = new PrizeCards();
            this.discardPile = new DiscardPile();
            this.hand = new Hand();
            this.username = string.Empty;
        }

        public void OnGainCardIntoCollection(Card3D cardAdded, PlayerCardOwner playerCardOwner)
        {
            if (playerCardOwner.GetType() == typeof(DeckController))
            {
                deck.OnCardAdded(cardAdded);
            }
            else if (playerCardOwner.GetType() == typeof(HandFanController))
            {
                hand.OnCardAdded(cardAdded);
            }
        }

        public void OnRemovedCardFromCollection(Card3D cardRemoved, PlayerCardOwner playerCardOwner)
        {
            if (playerCardOwner.GetType() == typeof(DeckController))
            {
                deck.OnCardRemoved(cardRemoved);
            }
            else if (playerCardOwner.GetType() == typeof(HandFanController))
            {
                hand.OnCardRemoved(cardRemoved);
            }
        }
    }
}
