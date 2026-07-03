using UnityEngine;
using UnityEngine.Events;
using WCore;

namespace Tito.Services.IAP
{
    public class IAPManager: Singleton<IAPManager>
    {
        [Header("IAP Providers")] 
        [SerializeField]
        private IAPProvider provider;
        
        [SerializeField] private IAPCatalog catalog;
        
        [SerializeField] private bool initializeOnStart;
        
        public UnityEvent OnInitialized;
        public UnityEvent OnInitializationFailed;

        private void Start()
        {
            if (initializeOnStart)
            {
                provider.Initialize(catalog);
            }
            
            provider.Initialized += OnProviderInitialized;
            provider.InitializeFailed += OnProviderInitializeFailed;
        }

        private void OnProviderInitialized()
        {
            OnInitialized?.Invoke();
        }

        private void OnProviderInitializeFailed()
        {
            OnInitializationFailed?.Invoke();
        }
        
        public void Purchase(string productId)
        {
            provider.Purchase(productId);
        }

        public void RestorePurchases()
        {
            provider.RestorePurchases();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Purchase(catalog.Products[0].Id);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RestorePurchases();
            }
        }
    }
}