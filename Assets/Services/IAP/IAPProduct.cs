using System;
using UnityEngine;

namespace Tito.Services.IAP
{
    [Serializable]
    public class IAPProduct
    {
        [SerializeField]
        private string _id;

        [SerializeField]
        private ProductType _type;

        [SerializeField]
        private bool _enabled = true;

        [SerializeField] 
        private float defaultPrice;
        
        [SerializeField]
        private string _displayName;

        [SerializeField]
        private string _description;
        
        [SerializeField]
        private Sprite _icon;
        
        [SerializeField]
        private bool _restoreOnLogin = true;

        public string Id => _id;

        public ProductType Type => _type;

        public bool Enabled => _enabled;
        
        public float DefaultPrice => defaultPrice;
        
        public string DisplayName => _displayName;
        public string Description => _description;
        public Sprite Icon => _icon;
        public bool RestoreOnLogin => _restoreOnLogin;
    }
}