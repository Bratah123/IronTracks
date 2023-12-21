using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCGLDeckTracker.CardCollection
{   
    // A Card represented by the PTCGL Client
    // This Card class does not represent each single card in a deck as it can count for multiple cards
    internal class Card
    {
        public string cardId { get; set; }
        public string quantity { get; set; }
        public string englishName { get; set; }

        public Card() { }


    }
}
