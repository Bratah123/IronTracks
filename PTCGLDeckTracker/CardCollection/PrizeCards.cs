using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCGLDeckTracker.CardCollection
{
    internal class PrizeCards : CardCollection
    {
        public Dictionary<string, Card> knownPrizeCards { get; set; }

        public override void OnCardAdded(Card3D cardAdded)
        {
            base.OnCardAdded(cardAdded);
            Melon<IronTracks>.Logger.Msg("Added Card " + cardAdded.name + " into hand.");
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            base.OnCardRemoved(cardRemoved);
            Melon<IronTracks>.Logger.Msg("Added Card " + cardRemoved.name + " into hand.");
        }

        public int GetPrizeCount()
        {
            return _cardCount;
        }

        public string PrizeCardStringForRender()
        {
            return "Total Prize Cards: " + _cardCount;
        }
    }
}
