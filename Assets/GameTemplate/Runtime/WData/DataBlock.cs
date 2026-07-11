using System;
using WCore;

namespace WData
{
    [Serializable]
    public class DataBlock<T> where T : DataBlock<T>
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = DataFileHandler.LoadFromDevice<T>(typeof(T).ToString());

                    if (_instance == null)
                        _instance = (T)Activator.CreateInstance(typeof(T));

                    _instance.Init();
                }

                return _instance;
            }
        }

        protected virtual void Init()
        {
            AppLifeCycle.Instance.OnApplicationPauseEvent += AppLifeCycle_ApplicationOnPause;
            AppLifeCycle.Instance.OnApplicationQuitEvent += AppLifeCycle_ApplicationOnQuit;
        }
        private void AppLifeCycle_ApplicationOnQuit()
        {
            Save();
        }

        private void AppLifeCycle_ApplicationOnPause(bool paused)
        {
            if (paused)
                Save();
        }

        public static void Save()
        {
            DataFileHandler.SaveToDevice(Instance, typeof(T).ToString());
        }

        public static void Delete()
        {
            _instance = null;

            DataFileHandler.DeleteInDevice(typeof(T).ToString());
        }
    }
}