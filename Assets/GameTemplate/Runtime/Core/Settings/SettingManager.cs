using GameTemplate.Runtime.Core.WCore.EventBus;
using Newtonsoft.Json;
using UnityEngine;
using WCore;

namespace GameTemplate.Runtime.Core.Settings
{
    [System.Serializable]
    public class GameSettings
    {
        public bool Music = true;
        public bool Sfx = true;
        public bool Haptic = true;
    }
    public class SettingManager: Singleton<SettingManager>
    {
        [SerializeField] private GameSettings settings;
        [SerializeField] private bool syncOnStart = true;

        private void Start()
        {
            if (syncOnStart)
            {
                SyncSettings();
            }
        }

        public void SetMusic(bool music)
        {
            if (settings.Music == music)
            {
                return;
            }
            
            settings.Music = music;
            SaveSettings();
        }

        public void SetSfx(bool sfx)
        {
            if (settings.Sfx == sfx)
            {
                return;
            }
            
            settings.Sfx = sfx;
            SaveSettings();
        }

        public void SetHaptic(bool haptic)
        {
            if (settings.Haptic == haptic)
            {
                return;
            }
            settings.Haptic = haptic;
            SaveSettings();
        }   

        private void SaveSettings()
        {
            PlayerPrefs.SetString("Settings", JsonConvert.SerializeObject(settings));
            EventBus<SettingEvent>.Post(new SettingEvent(settings));
        }
        
        public void SyncSettings()
        {
            settings = JsonConvert.DeserializeObject<GameSettings>(PlayerPrefs.GetString("Settings")) ?? new GameSettings();
            SetMusic(settings.Music);
            SetSfx(settings.Sfx);
            SetHaptic(settings.Haptic);
            SaveSettings();
        }
    }
}