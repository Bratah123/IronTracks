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

            foreach (var card in deckRenderOrder) {
                deckString += _cards[card] + " " + card + "\n";
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
            foreach (KeyValuePair<string, int> kvp in _cards)
            {
                total += kvp.Value;
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
                _cards[cdr.EnglishCardName] = quantity;
                _cardsWithId[cardID] = quantity;

                if (cdr.IsPokemonCard())
                {
                    pokemons.Add(cdr.EnglishCardName);
                }
                else if (cdr.IsTrainerCard())
                {
                    trainers.Add(cdr.EnglishCardName);
                }
                else
                {
                    energies.Add(cdr.EnglishCardName);
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
    }
}
