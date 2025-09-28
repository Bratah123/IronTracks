using Harmony;
using MelonLoader;
using System.Collections.Generic;
using System.Linq;
using TPCI.Rainier.Match.Cards;
using UnityEngine;

namespace PTCGLDeckTracker.CardCollection
{
    internal class Deck : CardCollection
    {
        public PrizeCards prizeCards { get; set; } = new PrizeCards();

        private string _deckOwner = "";

        List<string> deckRenderOrder = new List<string>();
        private Dictionary<string, TrackedCard> _currentCardsInDeck;

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

        public List<TrackedCard> GetCardsForRender()
        {
            var cards = new List<TrackedCard>();
            foreach (var cardID in deckRenderOrder)
            {
                if (!_currentCardsInDeck.ContainsKey(cardID))
                {
                    continue;
                }
                TrackedCard card = _currentCardsInDeck[cardID];
                if (card.card.quantity == 0)
                {
                    continue;
                }
                cards.Add(card);
            }
            return cards;
        }

        public int GetAssumedTotalQuantityOfCards()
        {
            int total = 0;
            foreach (KeyValuePair<string, TrackedCard> kvp in _currentCardsInDeck)
            {
                total += kvp.Value.card.quantity;
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

                _cards[cardID] = new TrackedCard(card);
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
            _currentCardsInDeck = _cards.ToDictionary(entry => entry.Key, entry => new TrackedCard(entry.Value));
        }

        private void AddCardToCurrentDeck(Card3D cardAdded)
        {
            if (string.IsNullOrEmpty(cardAdded.entityID) || cardAdded.entityID.Equals("PRIVATE"))
            {
                return;
            }

            if (!string.IsNullOrEmpty(cardAdded.cardSourceID))
            {
                var trackedCard = _currentCardsInDeck[cardAdded.cardSourceID];
                trackedCard.card.quantity++;
                trackedCard.highlightState = HighlightState.Added;
                trackedCard.highlightEndTime = Time.time + 2.0f;
            }
        }

        private void RemoveCardFromCurrentDeck(Card3D cardRemoved)
        {
            if (string.IsNullOrEmpty(cardRemoved.entityID) || cardRemoved.entityID.Equals("PRIVATE"))
            {
                return;
            }
            if (!string.IsNullOrEmpty(cardRemoved.cardSourceID))
            {
                var trackedCard = _currentCardsInDeck[cardRemoved.cardSourceID];
                if (trackedCard.card.quantity > 0)
                {
                    trackedCard.card.quantity--;
                    trackedCard.highlightState = HighlightState.Removed;
                    trackedCard.highlightEndTime = Time.time + 2.0f;
                }
            }
            var assumedTotal = GetAssumedTotalQuantityOfCards();
            if (_cardCount == 0 && assumedTotal != 0 && assumedTotal != prizeCards.GetKnownPrizeCardsCount())
            {
                if (assumedTotal == prizeCards.GetPrizeCount())
                {
                    foreach (var kvp in _currentCardsInDeck)
                    {
                        if (kvp.Value.card.quantity != 0)
                        {
                            var cardID = kvp.Value.card.cardID;
                            var assumedPrizeCard = kvp.Value.card;
                            prizeCards.KnownPrizeCards.Add(cardID, new TrackedCard(new Card(assumedPrizeCard)));
                            kvp.Value.card.quantity = 0;
                        }
                    }
                }
                else if (assumedTotal == (prizeCards.GetPrizeCount() + prizeCards.GetRemovedPrizeCardsCount()))
                {
                    foreach (var kvp in prizeCards.RemovedPrizedCards)
                    {
                        var removedPrizeCard = kvp.Value;
                        _currentCardsInDeck[kvp.Key].card.quantity -= removedPrizeCard.card.quantity;
                    }
                    foreach (var kvp in _currentCardsInDeck)
                    {
                        if (kvp.Value.card.quantity != 0)
                        {
                            var cardID = kvp.Value.card.cardID;
                            var assumedPrizeCard = kvp.Value.card;
                            prizeCards.KnownPrizeCards.Add(cardID, new TrackedCard(new Card(assumedPrizeCard)));
                            kvp.Value.card.quantity = 0;
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