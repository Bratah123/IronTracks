namespace PTCGLDeckTracker.CardCollection
{
    public enum HighlightState
    {
        None,
        Added,
        Removed
    }

    internal class TrackedCard
    {
        public Card card { get; }
        public HighlightState highlightState = HighlightState.None;
        public float highlightEndTime = 0f;

        public TrackedCard(Card card)
        {
            this.card = card;
        }
        
        public TrackedCard(TrackedCard trackedCard)
        {
            this.card = new Card(trackedCard.card);
            this.highlightState = trackedCard.highlightState;
            this.highlightEndTime = trackedCard.highlightEndTime;
        }
    }
}
