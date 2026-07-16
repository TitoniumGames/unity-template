using GameTemplate.Runtime.Core.Settings;
using GameTemplate.Runtime.GameData;
using UnityEngine;
using WCore;
using WPuzzle;

namespace GameTemplate.Runtime.Core
{
    public class GameManager: Singleton<GameManager>
    {
        [SerializeField] private ApplicationSettings applicationSettings;
        
        public bool IsInitializeOnStart => true;

        protected override void Awake()
        {
            base.Awake();

            if (IsInitializeOnStart)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (applicationSettings == null)
            {
                applicationSettings = ScriptableObject.CreateInstance<ApplicationSettings>();
            }

            Application.targetFrameRate = applicationSettings.TargetFPS;

            // Sync systems
            SettingManager.Instance.Sync();
        }
    }
}