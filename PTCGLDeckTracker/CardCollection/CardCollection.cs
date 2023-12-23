﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCGLDeckTracker.CardCollection
{
    internal abstract class CardCollection
    {
        // card_id : Card
        protected Dictionary<string, Card> _cards = new Dictionary<string, Card>();

        // card_id : quantity
        protected Dictionary<string, int> _cardsWithId = new Dictionary<string, int>();

        public CardCollection() { }

        virtual public void Clear()
        {
            _cards.Clear();
            _cardsWithId.Clear();
        }

        /// <summary>
        /// Called whenever a Card is Added back into the physical card collection (hand, discard, lost zone, etc..) in game.
        /// Used by us to keep track internally of cards.
        /// </summary>
        /// <param name="cardID"></param>
        abstract public void OnCardAdded(Card3D cardAdded);

        /// <summary>
        /// Called whenever a Card is removed from the physical card collection (hand, discard, deck, etc..) in game.
        /// Used by us to keep track internally of cards.
        /// </summary>
        /// <param name="cardID"></param>
        abstract public void OnCardRemoved(Card3D cardRemoved);
    }
}
