using Harmony;
using MelonLoader;
using System.Collections.Generic;

namespace PTCGLDeckTracker.CardCollection
{
    /// <summary>
    /// Deck will automatically assume all 60 cards are present in deck at all times
    /// unless it ABSOLUTELY knows what is prized (via deck search), etc..
    /// </summary>
    internal class Deck : CardCollection
    {
        private string _deckOwner = "";

        /// <summary>
        /// deckRenderOrder lets us know in what order the cards should be rendered in a list.
        /// This list should never be mutated.
        /// </summary>
        List<string> deckRenderOrder = new List<string>();
        /// <summary>
        /// This dictionary keeps track of the current expected cards in deck
        /// Transient data that is mutated.
        /// </summary>
        private Dictionary<string, Card> _currentCardsInDeck;
        private List<string> _knownCards = new List<string>();
        private int _deckCount = 0;

        public Deck(string deckOwner)
        {
            _deckOwner = deckOwner.Trim();
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

            int totalAssumedCards = GetAssumedTotalQuantityOfCards();

            foreach (var cardID in deckRenderOrder) {
                if (!_currentCardsInDeck.ContainsKey(cardID))
                {
                    continue;
                }
                Card card = _currentCardsInDeck[cardID];
                if (card.quantity == 0)
                {
                    continue;
                }
                deckString += card.quantity + " " + card;
                if (totalAssumedCards != _knownCards.Count)
                {
                    deckString += " (?)";
                }
                deckString += "\n";
            }

            deckString += "\nTotal Cards in Deck: " + GetTotalQuantityOfCards();
            deckString += "\nTotal ASSUMED Cards in Deck: " + totalAssumedCards;

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
            deckString += "\nTotal ASSUMED Cards in Deck: " + GetAssumedTotalQuantityOfCards();

            return deckString;
        }

        public int GetAssumedTotalQuantityOfCards()
        {
            int total = 0;
            foreach (KeyValuePair<string, Card> kvp in _currentCardsInDeck)
            {
                total += kvp.Value.quantity;
            }
            return total;
        }

        public int GetQuantityOfKnownCards()
        {
            return _knownCards.Count;
        }

        public int GetTotalQuantityOfCards()
        {
            return _deckCount;
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
            _currentCardsInDeck = new Dictionary<string, Card>(_cards);
        }

        private void UpdateCardQuantityInDeck(string cardID, int quantity)
        {
            foreach (var kvp in _cards)
            {
                Card card = kvp.Value;
                if (card.cardID == cardID)
                {
                   card.quantity = quantity;
                   break;
                }
            }
        }

        private void AddCardToCurrentDeck(Card3D cardAdded)
        {
            _deckCount++;
            // Ignore any cards added into our deck that is "unknown"
            // Typically occurs during setting up phase, when the deck is populated with 60 private cards
            if (string.IsNullOrEmpty(cardAdded.entityID) || cardAdded.entityID.Equals("PRIVATE"))
            {
                return;
            }

            if (!string.IsNullOrEmpty(cardAdded.cardSourceID))
            {
                var cardInDeck = _currentCardsInDeck[cardAdded.cardSourceID];
                cardInDeck.quantity++;
            }
        }

        private void RemoveCardFromCurrentDeck(Card3D cardRemoved)
        {
            _deckCount--;
            // Ignore any cards removed from our deck that is "unknown"
            // This is where the assumption is made that no cards are prized
            // AFAIK, the only time private cards are removed from the deck is during prizing.
            if (string.IsNullOrEmpty(cardRemoved.entityID) || cardRemoved.entityID.Equals("PRIVATE"))
            {
                return;
            }
            // We assume the deck already contains the keys to every card, so no need for null checks
            if (!string.IsNullOrEmpty(cardRemoved.cardSourceID))
            {
                var cardInDeck = _currentCardsInDeck[cardRemoved.cardSourceID];
                if (cardInDeck.quantity > 0)
                {
                    cardInDeck.quantity--;
                }
            }
        }

        public override void OnCardAdded(Card3D cardAdded)
        {
            Melon<IronTracks>.Logger.Msg("Added Card: " + Card.GetEnglishNameFromCard3DName(cardAdded.name) + " into deck.");
            AddCardToCurrentDeck(cardAdded);
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            Melon<IronTracks>.Logger.Msg("Removed Card: " + Card.GetEnglishNameFromCard3DName(cardRemoved.name) + " from deck.");
            RemoveCardFromCurrentDeck(cardRemoved);
        }
    }
}
