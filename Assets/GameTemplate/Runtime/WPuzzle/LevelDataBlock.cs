using MemoryPack;
using WData;

namespace WPuzzle
{
    [MemoryPackable]
    public partial class LevelDataBlock : DataBlock<LevelDataBlock>
    {
        [MemoryPackInclude] private int _currentLevelNumber;
        public static int CurrentLevelNumber => Instance._currentLevelNumber;
        
        protected override void Init()
        {
            base.Init();
            if (_currentLevelNumber < 1)
            {
                _currentLevelNumber = 1;
            }
        }
        public static void SetCurrentLevelNumber(int levelNumber)
        {
            Instance._currentLevelNumber = levelNumber;
            Save();
        }
    }
}