using GameTemplate.Runtime.Core.WCore.EventBus;
using MemoryPack;
using WData;

namespace GameTemplate.Runtime.Core.Settings
{
    [MemoryPackable]
    public partial class GameSettings
    {
        public bool Music { get; set; } = true;
        public bool Sfx { get; set; } = true;
        public bool Haptic { get; set; } = true;
    }

    public sealed class SettingManager
        : DataBlock<SettingManager, GameSettings>
    {
        public bool Music => Data.Music;
        public bool Sfx => Data.Sfx;
        public bool Haptic => Data.Haptic;

        public void SetMusic(bool value)
        {
            if (Data.Music == value)
                return;

            Data.Music = value;
            SaveAndNotify();
        }

        public void SetSfx(bool value)
        {
            if (Data.Sfx == value)
                return;

            Data.Sfx = value;
            SaveAndNotify();
        }

        public void SetHaptic(bool value)
        {
            if (Data.Haptic == value)
                return;

            Data.Haptic = value;
            SaveAndNotify();
        }

        private void SaveAndNotify()
        {
            Save();
            EventBus<SettingEvent>.Post(new SettingEvent(Data));
        }

        public void Sync()
        {
            EventBus<SettingEvent>.Post(new SettingEvent(Data));
        }
    }
}