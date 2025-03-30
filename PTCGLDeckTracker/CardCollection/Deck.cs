﻿using Harmony;
using MelonLoader;
using System.Collections.Generic;
using TPCI.Rainier.Match.Cards;

namespace PTCGLDeckTracker.CardCollection
{
    /// <summary>
    /// Deck will automatically assume all 60 cards are present in deck at all times
    /// unless it ABSOLUTELY knows what is prized (via deck search), etc..
    /// It also handles prize cards deducing.
    /// </summary>
    internal class Deck : CardCollection
    {
        public PrizeCards prizeCards { get; set; } = new PrizeCards();

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

        public Deck(string deckOwner)
        {
            _deckOwner = deckOwner.Trim();
        }

        override public void Clear()
        {
            base.Clear();
            deckRenderOrder.Clear();
            prizeCards.Clear();
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
                if (totalAssumedCards != _cardCount)
                {
                    deckString += " (?)";
                }
                deckString += "\n";
            }

            deckString += "\nTotal Cards in Deck: " + _cardCount;
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

        public int GetTotalQuantityOfCards()
        {
            return _cardCount;
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
            var assumedTotal = GetAssumedTotalQuantityOfCards();
            // When players deck search, deduce the prize cards
            if (_cardCount == 0 && assumedTotal != 0 && assumedTotal != prizeCards.GetKnownPrizeCardsCount())
            {
                if (assumedTotal == prizeCards.GetPrizeCount())
                {
                    // Move all the current cards still "assumed" left in the deck to prize cards
                    foreach (var kvp in _currentCardsInDeck)
                    {
                        // All cards with a quantity of non-zero value during a deck search is considered a prized card
                        if (kvp.Value.quantity != 0)
                        {
                            var cardID = kvp.Value.cardID;
                            var assumedPrizeCard = kvp.Value;
                            prizeCards.KnownPrizeCards.Add(cardID, new Card(assumedPrizeCard));
                            assumedPrizeCard.quantity = 0;
                        }
                    }
                }
                // This targets the edge case of when a player takes a prize card without ever having deck searched
                else if (assumedTotal == (prizeCards.GetPrizeCount() + prizeCards.GetRemovedPrizeCardsCount()))
                {
                    // Remove the appropriate cards from the "assumed" deck
                    foreach (var kvp in prizeCards.RemovedPrizedCards)
                    {
                        var removedPrizeCard = kvp.Value;
                        _currentCardsInDeck[kvp.Key].quantity -= removedPrizeCard.quantity;
                    }
                    // Move all the current cards still "assumed" left in the deck to prize cards
                    foreach (var kvp in _currentCardsInDeck)
                    {
                        // All cards with a quantity of non-zero value during a deck search is considered a prized card
                        if (kvp.Value.quantity != 0)
                        {
                            var cardID = kvp.Value.cardID;
                            var assumedPrizeCard = kvp.Value;
                            prizeCards.KnownPrizeCards.Add(cardID, new Card(assumedPrizeCard));
                            assumedPrizeCard.quantity = 0;
                        }
                    }
                }
            }
        }

        public override void OnCardAdded(Card3D cardAdded)
        {
            base.OnCardAdded(cardAdded);
            Melon<IronTracks>.Logger.Msg("Added Card: " + Card.GetEnglishNameFromCard3DName(cardAdded.name) + " into deck.");
            AddCardToCurrentDeck(cardAdded);
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            base.OnCardRemoved(cardRemoved);
            Melon<IronTracks>.Logger.Msg("Removed Card: " + Card.GetEnglishNameFromCard3DName(cardRemoved.name) + " from deck.");
            RemoveCardFromCurrentDeck(cardRemoved);
        }
    }
}
