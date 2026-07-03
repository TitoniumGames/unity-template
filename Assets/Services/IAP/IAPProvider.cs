using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tito.Services.IAP
{
    public abstract class IAPProvider: ScriptableObject, IIAPProvider
    {
        public bool IsInitialized { get; set; }
        public abstract Action Initialized { get; set; }
        public abstract Action InitializeFailed { get; set; }
        public abstract UniTask Initialize(IAPCatalog catalog);
        public abstract UniTask<PurchaseResult> Purchase(string productId);
        public abstract UniTask RestorePurchases();
        public abstract bool IsPurchased(string productId);
        public abstract string GetLocalizedPrice(string productId);
        public abstract decimal GetPrice(string productId);
        public abstract string GetCurrencyCode(string productId);
    }
}