using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTCGLDeckTracker.CardCollection
{
    internal class DiscardPile : CardCollection
    {
        public override void OnCardAdded(Card3D cardAdded)
        {
            Melon<IronTracks>.Logger.Msg("Added Card " + cardAdded.name + " into discard pile.");
        }

        public override void OnCardRemoved(Card3D cardRemoved)
        {
            Melon<IronTracks>.Logger.Msg("Removed Card " + cardRemoved.name + " from discard pile.");
        }
    }
}
