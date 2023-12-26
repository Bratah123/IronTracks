using Harmony;
using MelonLoader;
using System.Collections.Generic;

namespace PTCGLDeckTracker.CardCollection
{
    internal class Deck : CardCollection
    {
        private string _deckOwner = "";

        /// <summary>
        /// deckRenderOrder lets us know in what order the cards should be rendered in a list.
        /// This list should never be mutated.
        /// </summary>
        List<string> deckRenderOrder = new List<string>();
        private List<string> _knownCards = new List<string>();
        /// <summary>
        /// This dictionary keeps track of the current expected cards in deck
        /// Transient data that is mutated.
        /// </summary>
        private Dictionary<string, Card> _currentCardsInDeck = new Dictionary<string, Card>();

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
                if (_cards[cardID] == null)
                {
                    continue;
                }
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

        private void DecrementCardQuantity(string cardID)
        {
            foreach (var kvp in _cards)
            {
                Card card = kvp.Value;
                if (card.cardID == cardID)
                {
                    if (card.quantity > 0)
                    {
                        card.quantity--;
                    }
                    break;
                }
            }
        }

        private void IncrementCardQuantity(string cardID)
        {
            foreach (var kvp in _cards)
            {
                Card card = kvp.Value;
                if (card.cardID == cardID)
                {
                    if (card.quantity > 0)
                    {
                        card.quantity++;
                    }
                    break;
                }
            }
        }

        private void AddCardToCurrentDeck(Card3D cardAdded)
        {
            var currentCardInDeck = _currentCardsInDeck[cardAdded.cardSourceID];
            if (currentCardInDeck == null) // If the card didn't already exist in the deck
            {
                // cardAdded.cardSourceID could be "", an empty string if the card is a PRIVATE card
                // TCPI does this to indicate when a card is not yet "known" from a player.
                var cardToAdd = new Card(cardAdded.cardSourceID);
                cardToAdd.quantity = 1;
                cardToAdd.englishName = Card.GetEnglishNameFromCard3DName(cardAdded.name);
                _currentCardsInDeck[cardAdded.cardSourceID] = cardToAdd;
            }
            else
            {
                currentCardInDeck.quantity++;
            }
        }

        private void RemoveCardFromCurrentDeck(Card3D cardRemoved)
        {
            var currentCardInDeck = _currentCardsInDeck[cardRemoved.cardSourceID];
            if (currentCardInDeck != null)
            {
                if (currentCardInDeck.quantity > 0)
                {
                    currentCardInDeck.quantity--;
                }

                // If the quantity reaches 0 after being removed, delete it from the dictionary
                if (currentCardInDeck.quantity == 0)
                {
                    _currentCardsInDeck.Remove(cardRemoved.cardSourceID);
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
        }
    }
}
