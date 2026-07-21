using GameTemplate.Runtime.Core.WCore.EventBus;

namespace GameTemplate.Runtime.GameData
{
    public struct PlayerCurrencyChangedEvent: IEvent
    {
        public CurrencyType CurrencyType { get; }
        public double Value { get; }
        
        public PlayerCurrencyChangedEvent(CurrencyType currencyType, double value)
        {
            CurrencyType = currencyType;
            Value = value;
        }
    }
}