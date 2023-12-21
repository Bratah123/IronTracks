using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PTCGLDeckTracker.CardCollection;

namespace PTCGLDeckTracker
{
    internal class Player
    {
        public string username { get; set; }

        // Card Collections
        Deck deck;
        PrizeCards prizeCards;
        DiscardPile discardPile;
        Hand hand;

        public Player()
        {
            this.deck = new Deck("playerOne");
            this.username = string.Empty;
        }

        public Deck GetDeck()
        {
            return this.deck;
        }
    }
}
