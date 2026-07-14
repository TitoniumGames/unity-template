using GameTemplate.Runtime.Core.WCore.EventBus;

namespace GameTemplate.Runtime.GameData
{
    public struct PlayerCurrencyChangedEvent: IEvent
    {
        public CurrencyType CurrencyType { get; }
        public int Coin { get; }
        
        public PlayerCurrencyChangedEvent(CurrencyType currencyType, int coin)
        {
            CurrencyType = currencyType;
            Coin = coin;
        }
    }
}