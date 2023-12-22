using System.Collections.Generic;

namespace PTCGLDeckTracker.CardCollection
{
    internal class Deck : CardCollection
    {
        private string _deckOwner = "";

        /// <summary>
        /// Cards below here are transient data, and are mutated throughout the game
        /// </summary>
        List<string> deckRenderOrder = new List<string>();

        public Deck(string deckOwner)
        {
            this._deckOwner = deckOwner.Trim();
        }

        override public void Clear()
        {
            base.Clear();
            deckRenderOrder.Clear();
        }

        public string GetDeckOwner()
        {
            return _deckOwner;
        }

        public void SetDeckOwner(string deckOwner)
        {
            this._deckOwner = deckOwner;
        }

        public string DeckStringForRender()
        {
            string deckString = "";

            if (_cardsWithId.Count == 0)
            {
                return deckString;
            }

            foreach (var cardID in deckRenderOrder) {
                deckString += _cards[cardID].quantity + " " + _cards[cardID] + "\n";
            }

            deckString += "\nTotal Cards in Deck: " + GetTotalQuantityOfCards();

            return deckString;
        }

        /// <summary>
        /// For debug purposes, this will return a string that contains the decklist
        /// but instead of english card names, it's their PTCGL card ids.
        /// </summary>
        /// <returns>String</returns>
        public string DeckStringWithIds()
        {
            string deckString = "";

            if (_cardsWithId.Count == 0)
            {
                return deckString;
            }

            foreach (var kvp in _cardsWithId)
            {
                deckString += kvp.Value + " " + kvp.Key + "\n";
            }

            deckString += "\nTotal Cards in Deck: " + GetTotalQuantityOfCards();

            return deckString;
        }

        public int GetTotalQuantityOfCards()
        {
            int total = 0;
            foreach (KeyValuePair<string, Card> kvp in _cards)
            {
                total += kvp.Value.quantity;
            }
            return total;
        }

        public void PopulateDeck(Dictionary<string, int> deck)
        {
            Clear();

            var pokemons = new List<string>();
            var trainers = new List<string>();
            var energies = new List<string>();

            foreach (var pair in deck)
            {
                var quantity = pair.Value;
                var cardID = pair.Key;

                CardDatabase.DataAccess.CardDataRow cdr = ManagerSingleton<CardDatabaseManager>.instance.TryGetCardFromDatabase(cardID);

                var card = new Card(cardID);
                card.quantity = quantity;
                card.englishName = cdr.EnglishCardName;
                card.setID = cdr.CardSetID;

                _cards[cardID] = card;
                _cardsWithId[cardID] = quantity;

                if (cdr.IsPokemonCard())
                {
                    pokemons.Add(cardID);
                }
                else if (cdr.IsTrainerCard())
                {
                    trainers.Add(cardID);
                }
                else
                {
                    energies.Add(cardID);
                }
            }

            foreach (var item in pokemons)
            {
                deckRenderOrder.Add(item);
            }
            foreach (var item in trainers)
            {
                deckRenderOrder.Add(item);
            }
            foreach (var item in energies)
            {
                deckRenderOrder.Add(item);
            }
        }

        public override void OnCardAdded(Card3D cardID)
        {
            throw new System.NotImplementedException();
        }

        public override void OnCardRemoved(Card3D cardID)
        {
            throw new System.NotImplementedException();
        }
    }
}
