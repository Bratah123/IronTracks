using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCGLDeckTracker.CardCollection
{
    internal class CardCollection
    {
        protected Dictionary<string, int> _cards = new Dictionary<string, int>();

        // card_id : quantity
        protected Dictionary<string, int> _cardsWithId = new Dictionary<string, int>();

        public CardCollection() { }

        virtual public void Clear()
        {
            _cards.Clear();
            _cardsWithId.Clear();
        }
    }
}
