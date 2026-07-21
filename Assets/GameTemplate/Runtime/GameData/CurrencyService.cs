using System;
using GameTemplate.Runtime.Core.WCore.EventBus;
using MemoryPack;

namespace GameTemplate.Runtime.GameData
{
    public enum CurrencyType { Coin, Gem }
    
    [MemoryPackable]
    public partial class CurrencyData
    {
        public double Coin;
        public double Gem;
    }
    
    public class CurrencyService: PlayerService<CurrencyData>
    {
        private readonly CurrencyData data;
        private readonly Action save;

        public CurrencyService(CurrencyData data, Action save) : base(data, save)
        {
            this.data = data;
            this.save = save;
        }

        public double Coin => data.Coin;

        public double Gem => data.Gem;

        public void AddCoin(double amount)
        {
            if (amount <= 0)
                return;

            data.Coin += amount;

            save();

            EventBus<PlayerCurrencyChangedEvent>.Post(
                new PlayerCurrencyChangedEvent(
                    CurrencyType.Coin,
                    data.Coin));
        }

        public bool SpendCoin(int amount)
        {
            if (amount <= 0)
                return false;

            if (data.Coin < amount)
                return false;

            data.Coin -= amount;

            save();

            EventBus<PlayerCurrencyChangedEvent>.Post(
                new PlayerCurrencyChangedEvent(
                    CurrencyType.Coin,
                    data.Coin));

            return true;
        }

        public bool HasEnoughCoin(int amount)
        {
            return data.Coin >= amount;
        }
    }
}