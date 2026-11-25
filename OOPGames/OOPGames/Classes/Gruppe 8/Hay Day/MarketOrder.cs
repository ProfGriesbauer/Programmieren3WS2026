namespace OOPGames
{
    public sealed class MarketOrder
    {
        public MarketOrder(string item, int quantity, int reward)
        {
            Item = item;
            Quantity = quantity;
            Reward = reward;
        }

        public string Item { get; private set; }
        public int Quantity { get; private set; }
        public int Reward { get; private set; }
        public string Description
        {
            get { return "Lieferanfrage: " + Quantity + " " + Item + " für " + Reward + " Münzen."; }
        }
    }
}
