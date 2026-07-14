using System;

namespace GameTemplate.Runtime.GameData
{
    public abstract class PlayerService<TData>
    {
        protected readonly TData Data;
        private readonly Action save;

        protected PlayerService(TData data, Action save)
        {
            Data = data;
            this.save = save;
        }

        protected void Save()
        {
            save();
        }
    }
}