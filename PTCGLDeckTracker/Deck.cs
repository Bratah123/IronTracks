using System.Collections.Generic;

namespace PTCGLDeckTracker
{
    internal class Deck
    {
        Dictionary<string, int> deck = new Dictionary<string, int>();
        Dictionary<string, int> deckWithIds = new Dictionary<string, int>();
        List<string> deckRenderOrder = new List<string>();
        private string deckOwner = "";

        public Deck(string deckOwner)
        {
            this.deckOwner = deckOwner.Trim();
        }

        public void Clear()
        {
            deck.Clear();
            deckWithIds.Clear();
            deckRenderOrder.Clear();
        }

        public string GetDeckOwner()
        {
            return deckOwner;
        }

        public void SetDeckOwner(string deckOwner)
        {
            this.deckOwner = deckOwner;
        }

        public string DeckStringForRender()
        {
            string deckString = "";

            if (deckWithIds.Count == 0)
            {
                return deckString;
            }

            foreach (var card in deckRenderOrder) {
                deckString += deck[card] + " " + card + "\n";
            }

            deckString += "\nTotal Cards in Deck: " + GetTotalQuantityOfCards();

            return deckString;
        }

        public int GetTotalQuantityOfCards()
        {
            int total = 0;
            foreach (KeyValuePair<string, int> kvp in deck)
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
                this.deck[cdr.EnglishCardName] = quantity;
                deckWithIds[cardID] = quantity;

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
