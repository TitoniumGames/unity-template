using UnityEngine;

namespace GameTemplate.Runtime.GameData
{
    [CreateAssetMenu(
        fileName = "BoosterConfig",
        menuName = "GameTemplate/Booster Config")]
    public class BoosterConfigSO : ScriptableObject
    {
        public string Id;

        public Sprite Icon;

        public string DisplayName;

        [TextArea]
        public string Description;

        public int DefaultAmount;
    }
}