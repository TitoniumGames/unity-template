using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Tito.Services.IAP
{
    public interface IIAPProvider
    {
        bool IsInitialized { get; set; }
        
        Action Initialized { get; set; }
        
        Action InitializeFailed { get; set; }

        UniTask Initialize(IAPCatalog catalog);

        UniTask<PurchaseResult> Purchase(string productId);

        UniTask RestorePurchases();

        bool IsPurchased(string productId);

        string GetLocalizedPrice(string productId);

        decimal GetPrice(string productId);

        string GetCurrencyCode(string productId);
        
    }
}
