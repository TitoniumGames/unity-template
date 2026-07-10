using GameTemplate.Runtime.Core.WCore.EventBus;

namespace GameTemplate.Runtime.Core.Settings
{
    public struct SettingEvent: IEvent
    {
        public GameSettings Settings { get; private set; }
        
        public SettingEvent(GameSettings settings)
        {
            Settings = settings;
        }
    }
}