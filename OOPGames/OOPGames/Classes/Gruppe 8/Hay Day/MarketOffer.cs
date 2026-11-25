namespace OOPGames
{
    public sealed class MarketOffer
    {
        public MarketOffer(string item, int price)
        {
            Item = item;
            Price = price;
        }

        public string Item { get; private set; }
        public int Price { get; private set; }
    }
}
