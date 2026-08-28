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

            // Process new purchases (từ user action - click buy)
            ProcessNewPurchase(pendingOrder);
        }
        
        /// <summary>
        /// Process new Purchase
        /// </summary>
        private UniTask ProcessNewPurchase(PendingOrder pendingOrder)
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
            
            Debug.Log($"Confirming new purchase: {product.definition.id}");
            m_StoreController.ConfirmPurchase(pendingOrder);
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// Xử lý purchase RESTORE (từ OnPurchasesFetched - khi app restart)
        /// </summary>
        private void ProcessRestoredPurchase(PendingOrder pendingOrder)
        {
            string receipt = pendingOrder.Info.Receipt;

            if (string.IsNullOrEmpty(receipt))
            {
                Debug.LogError("Restored purchase failed: Receipt is null or empty.");
                return;
            }

            var product = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product == null)
            {
                Debug.LogError("Restored purchase failed: Product is null.");
                return;
            }

            var transactionId = pendingOrder.Info.TransactionID;
            
            // ✅ Check duplicate chỉ cho restore purchases
            if (m_ProcessedTransactionIds.Contains(transactionId))
            {
                Debug.Log($"Restored purchase already processed: {transactionId}");
                return;
            }

            Debug.Log($"Confirming restored purchase: {product.definition.id}");
            m_ProcessedTransactionIds.Add(transactionId);
            m_StoreController.ConfirmPurchase(pendingOrder);
        }

        
        private void OnPurchaseFailed(FailedOrder failedOrder)
        {
            Debug.LogError($"OnPurchaseFailed: {failedOrder.Info}, reason: {failedOrder.FailureReason}");
            
            var product = failedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product == null)
            {
                Debug.LogError("OnPurchaseFailed: Product is null");
                m_IsPurchaseInProgress = false;
                return;
            }

            m_IsPurchaseInProgress = false;
            Debug.Log($"UnityIAPProvider: Purchase in progress flag reset");
            
            EventBus<PurchaseFailedEvent>.Post(
                new PurchaseFailedEvent(
                    product.definition.catalogListingId, 
                    PurchaseStatus.Failed, 
                    failedOrder.FailureReason.ToString()));
        }
        
        private void OnStoreConnected()
        {
            Debug.Log("UnityIAPProvider: Store connected");
            m_IsPurchaseInProgress = false;
        }
        
        private void OnPurchaseConfirmed(Order order)
        {
            if (order is FailedOrder failedOrder)
            {
                Debug.LogError($"Purchase failed for product: {failedOrder.Info}, reason: {failedOrder.FailureReason}");
                return;
            }

            var product = order.CartOrdered.Items().FirstOrDefault()?.Product;
            if (product == null)
            {
                Debug.LogError("OnPurchaseConfirmed: Product is null");
                m_IsPurchaseInProgress = false;
                return;
            }

            var transactionId = order.Info?.TransactionID;
            
            // ✅ Mark as processed (track both new purchases and restored ones)
            if (!string.IsNullOrEmpty(transactionId))
            {
                m_ProcessedTransactionIds.Add(transactionId);
                Debug.Log($"UnityIAPProvider: Purchase confirmed - Transaction tracked: {transactionId}");
            }

            Debug.Log($"UnityIAPProvider: Purchase confirmed for product {product.definition.id}");
            m_IsPurchaseInProgress = false;
            
            // ✅ Post event to grant access
            EventBus<PurchaseSuccessEvent>.Post(
                new PurchaseSuccessEvent(
                    product.definition.catalogListingId, 
                    transactionId, 
                    order.Info.Receipt));
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription obj)
        {
            Debug.LogError($"UnityIAPProvider: Purchase fetch failed: {obj}");
        }

        private void OnPurchasesFetched(Orders orders)
        {
            Debug.Log("UnityIAPProvider: OnPurchasesFetched - Processing restored/confirmed purchases");
            
            // Handle confirmed orders (successfully completed purchases - for restoration)
            foreach (var confirmedOrder in orders.ConfirmedOrders)
            {
                var product = confirmedOrder.CartOrdered.Items().FirstOrDefault()?.Product;
                if (product == null)
                    continue;

                // For non-consumable products and subscriptions, handle restoration
                if (product.definition.type != UnityEngine.Purchasing.ProductType.Consumable)
                {
                    var transactionId = confirmedOrder.Info.TransactionID;
                    
                    // Skip if already processed in this session
                    if (m_ProcessedTransactionIds.Contains(transactionId))
                    {
                        Debug.Log($"Confirmed order already processed: {transactionId}");
                        continue;
                    }

                    Debug.Log($"Restored purchase: {product.definition.id} - {transactionId}");
                    m_ProcessedTransactionIds.Add(transactionId);
                    
                    // Post restore event to grant access
                    EventBus<PurchaseSuccessEvent>.Post(
                        new PurchaseSuccessEvent(
                            product.definition.catalogListingId,
                            transactionId,
                            confirmedOrder.Info.Receipt));
                }
            }

            // Handle pending orders that need confirmation (restore/retry scenarios)
            // These orders haven't been fulfilled yet and need to be confirmed
            foreach (var pendingOrder in orders.PendingOrders)
            {
                var product = pendingOrder.CartOrdered.Items().FirstOrDefault()?.Product;
                if (product == null)
                    continue;

                // Process as restored purchase (có duplicate check)
                ProcessRestoredPurchase(pendingOrder);
            }
        }

        private void OnProductsFetchFailed(ProductFetchFailed obj)
        {
            Debug.LogError($"UnityIAPProvider: Product fetch failed: {obj}");
        }

        private void OnProductsFetched(List<Product> obj)
        {
            Debug.Log($"UnityIAPProvider: Products fetched ({obj.Count} products)");
            
            // On Google Play: FetchPurchases() automatically restores owned products after reinstall
            // This will trigger OnPurchasesFetched which handles the restoration flow
            m_StoreController.FetchPurchases();
            Debug.Log("UnityIAPProvider: Fetching purchases to restore owned products");
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
            if (product == null)
            {
                purchase.Status = PurchaseStatus.ProductNotFound;
                Debug.LogError($"Product with ID {productId} not found in the store.");
                return UniTask.FromResult(purchase);
            }

            // ✅ Check: Non-consumable product đã owned chưa?
            if (product.definition.type != UnityEngine.Purchasing.ProductType.Consumable)
            {
                if (IsPurchased(productId))
                {
                    Debug.LogWarning($"Product {productId} already owned. Cannot purchase again.");
                    purchase.Status = PurchaseStatus.Failed;
                    purchase.Error = "This item has already been purchased.";
                    return UniTask.FromResult(purchase);
                }
            }

            // ✅ Proceed with purchase
            m_IsPurchaseInProgress = true;
            m_StoreController.PurchaseProduct(product);
            purchase.Status = PurchaseStatus.Pending;
            
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
                    Debug.Log("UnityIAPProvider: RestoreTransactions successful - Now fetching purchases");
                    
                    // According to Unity IAP documentation:
                    // After RestoreTransactions succeeds, we must call FetchPurchases() to retrieve
                    // all restored purchases. This will trigger OnPurchasesFetched with all transactions.
                    m_StoreController.FetchPurchases();
                    
                    // Note: The actual restore event will be posted from OnPurchasesFetched
                    // after all purchases have been processed
                }
                else
                {
                    Debug.LogError($"UnityIAPProvider: Restore purchases failed: {error}");
                    EventBus<RestorePurchaseEvent>.Post(new RestorePurchaseEvent(false, error));
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