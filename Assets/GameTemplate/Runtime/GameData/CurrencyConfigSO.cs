using UnityEngine;

namespace GameTemplate.Runtime.GameData
{
    [CreateAssetMenu(menuName = "GameTemplate/Currency Config", fileName = "Currency Config")]
    public class CurrencyConfigSO: ScriptableObject
    {
        public CurrencyType CurrencyType;
        public Sprite Sprite;
    }
}