namespace Imgeneus.Game.Market
{
    public enum MarketBuyItemResult : byte
    {
        Ok = 0,
        AuctionClosed = 1,
        MyAuction = 2,
        NotEnoughMoney = 3
    }
}
