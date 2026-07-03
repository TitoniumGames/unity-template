using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GameTemplate.Runtime.Core.WCore.EventBus;
using Tito.Services.IAP.Events;
using UnityEngine;
using UnityEngine.Purchasing;


namespace Tito.Services.IAP.Provider
{
    [CreateAssetMenu(fileName = "UnityIAPProvider", menuName = "GameTemplate/IAP/UnityIAPProvider")]
    public class UnityIAPProvider: IAPProvider
    {
        public override Action Initialized { get; set; }
        public override Action InitializeFailed { get; set; }

        private StoreController m_StoreController;
        private bool m_IsPurchaseInProgress;
        private readonly HashSet<string> m_ProcessedTransactionIds = new HashSet<string>();
        
        public override async UniTask Initialize(IAPCatalog catalog)
        {
            var catalogProvider = new CatalogProvider();
            foreach (var product in catalog.Products)
            {
                if (product.Enabled)
                {
                    catalogProvider.AddProduct(product.Id, Convert(product.Type));
                }
            }
            // Get StoreController
            m_StoreController = UnityIAPServices.StoreController();

            // Add event listeners
            m_StoreController.OnStoreDisconnected += OnStoreDisconnected;
            m_StoreController.OnStoreConnected += OnStoreConnected;

            m_StoreController.OnProductsFetched += OnProductsFetched;
            m_StoreController.OnProductsFetchFailed += OnProductsFetchFailed;

            m_StoreController.OnPurchasesFetched += OnPurchasesFetched;
            m_StoreController.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            m_StoreController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_StoreController.OnPurchasePending += OnPurchasePending;
            m_StoreController.OnPurchaseFailed += OnPurchaseFailed;
            m_StoreController.OnPurchaseDeferred += OnPurchaseDeferred;

            // Connect to store
            await m_StoreController.Connect();
            catalogProvider.FetchProducts(list => m_StoreController.FetchProducts(list));
            Debug.Log("UnityIAPProvider: Initialized");
            IsInitialized = true;
            Initialized?.Invoke();
        }
        
        private void OnPurchaseDeferred(DeferredOrder order)
        {
            Debug.Log($"Purchase deferred: {order.Info}");
        }
        
        private void OnPurchasePending(PendingOrder pendingOrder)
        {
            Debug.Log($"Purchase pending for product: {pendingOrder.Info}");
            
            foreach (var product in pendingOrder.CartOrdered.Items())
            {
                Debug.Log($"Pending product: {product.CatalogListingId}, quantity: {product.Quantity}");
            }

            ProcessPurchase(pendingOrder);
        }
        
        private UniTask ProcessPurchase(PendingOrder pendingOrder)
        {
            string receipt = pendingOrder.Info.Receipt;

            if (string.IsNullOrEmpty(receipt))
            {
                Debug.LogError("Purchase failed: Receipt is null or empty.");
                return UniTask.CompletedTask;
            }

            var product = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product == null)
            {
                Debug.LogError("Purchase failed: Product is null.");
                return UniTask.CompletedTask;
            }

            var transitionId = pendingOrder.Info.TransactionID;
            if (m_ProcessedTransactionIds.Contains(transitionId))
            {
                Debug.Log($"Duplicate transaction ignored: {transitionId}");
                return UniTask.CompletedTask;
            }

            m_StoreController.ConfirmPurchase(pendingOrder);
            return UniTask.CompletedTask;
        }

        
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            Debug.Log($"Purchase failed for product: {failedOrder.Info}, reason: {failedOrder.FailureReason}");
            var product = failedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            m_IsPurchaseInProgress = false;
            EventBus<PurchaseFailedEvent>.Post(new PurchaseFailedEvent(product.definition.catalogListingId, PurchaseStatus.Failed, failedOrder.FailureReason.ToString()));
        }
        
        private void OnStoreConnected()
        {
            Debug.Log("UnityIAPProvider: Store connected");
        }
        
        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failedOrder)
            {
                Debug.LogError($"Purchase failed for product: {failedOrder.Info}, reason: {failedOrder.FailureReason}");
                return;
            }

            var product = order.CartOrdered.Items().FirstOrDefault()?.Product;

            // Mark as processed now that the full flow has completed successfully
            m_ProcessedTransactionIds.Add(order.Info?.TransactionID);

            Debug.Log($"UnityIAPProvider: Purchase confirmed for product {order.Info.Receipt}");
            m_IsPurchaseInProgress = false;
            EventBus<PurchaseSuccessEvent>.Post(new PurchaseSuccessEvent(product.definition.catalogListingId, order.Info.TransactionID, order.Info.Receipt));
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription obj)
        {
            Debug.LogError($"UnityIAPProvider: Purchase fetch failed: {obj}");
        }

        private void OnPurchasesFetched(Orders orders)
        {
            Debug.Log("UnityIAPProvider: OnPurchasesFetched");
            foreach (var confirmedOrder in orders.ConfirmedOrders)
            {
                if (confirmedOrder.CartOrdered.Items().FirstOrDefault()?.Product.definition.type !=
                    UnityEngine.Purchasing.ProductType.Consumable)
                {
                    // handle restore purchase for android
                }
            }
        }

        private void OnProductsFetchFailed(ProductFetchFailed obj)
        {
            Debug.LogError($"UnityIAPProvider: Product fetch failed: {obj}");
        }

        private void OnProductsFetched(List<Product> obj)
        {
            m_StoreController.FetchPurchases();
            Debug.Log("UnityIAPProvider: Products fetched");
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            Debug.LogError($"Store disconnected: {description.Message}");
            InitializeFailed.Invoke();
        }
        

        public override UniTask<PurchaseResult> Purchase(string productId)
        {
            var purchase = new PurchaseResult();
            if (m_IsPurchaseInProgress)
            {
                Debug.LogWarning("Purchase already in progress. Please wait for the current purchase to complete.");
                purchase.Status = PurchaseStatus.Failed;
                purchase.Error = "Purchase already in progress.";
                return UniTask.FromResult(purchase);
            }

            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                purchase.Status = PurchaseStatus.Failed;
                purchase.Error = "StoreController is not initialized.";
                return UniTask.FromResult(purchase);
            }
            if (!IsInitialized)
            {
                purchase.Status = PurchaseStatus.NotInitialized;
                return UniTask.FromResult(purchase);
            }
            var product = m_StoreController.GetProductById(productId);
            m_IsPurchaseInProgress = true;
            if (product != null)
            {
                m_StoreController.PurchaseProduct(product);
                purchase.Status = PurchaseStatus.Pending;
            }
            else
            {
                m_IsPurchaseInProgress = false;
                purchase.Status = PurchaseStatus.ProductNotFound;
            }
            return UniTask.FromResult(purchase);
        }

        public override UniTask RestorePurchases()
        {
            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                return UniTask.CompletedTask;
            }
            m_StoreController.RestoreTransactions((success, error) =>
            {
                if (success)
                {
                    Debug.Log("UnityIAPProvider: Restore purchases successful");
                }
                else
                {
                    Debug.LogError($"UnityIAPProvider: Restore purchases failed: {error}");
                }
            });
            return UniTask.CompletedTask;
        }

        public override bool IsPurchased(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                return false;
            }

            var activePurchases = m_StoreController.GetPurchases();
            foreach (var purchase in activePurchases)
            {
                // Dig into the purchase information details
                foreach (var purchasedProductInfo in purchase.Info.PurchasedProductInfo)
                {
                    string ownedProductId = purchasedProductInfo.productId;
                    if (ownedProductId == productId)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string GetLocalizedPrice(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                return string.Empty;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
            {
                return product.metadata.localizedPriceString;
            }
            else
            {
                Debug.LogWarning($"Product with ID {productId} not found.");
                return string.Empty;
            }
        }

        public override decimal GetPrice(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                return 0m;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
            {
                return product.metadata.localizedPrice;
            }
            else
            {
                Debug.LogWarning($"Product with ID {productId} not found.");
                return 0m;
            }
        }

        public override string GetCurrencyCode(string productId)
        {
            if (m_StoreController == null)
            {
                Debug.LogError("StoreController is not initialized. Please initialize the IAP provider first.");
                return string.Empty;
            }

            var product = m_StoreController.GetProductById(productId);
            if (product != null)
            {
                return product.metadata.isoCurrencyCode;
            }
            else
            {
                Debug.LogWarning($"Product with ID {productId} not found.");
                return string.Empty;
            }
        }
        
        private UnityEngine.Purchasing.ProductType Convert(ProductType type)
        {
            return type switch
            {
                ProductType.Consumable =>
                    UnityEngine.Purchasing.ProductType.Consumable,

                ProductType.NonConsumable =>
                    UnityEngine.Purchasing.ProductType.NonConsumable,

                ProductType.Subscription =>
                    UnityEngine.Purchasing.ProductType.Subscription,

                _ =>
                    UnityEngine.Purchasing.ProductType.Consumable
            };
        }
    }
}