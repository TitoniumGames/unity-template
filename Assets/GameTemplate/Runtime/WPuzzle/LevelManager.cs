using System;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using WCore;

namespace WPuzzle
{
    public class LevelManager : Singleton<LevelManager>
    {
        [Title("Required References")]
        [SerializeField] private LevelConfigSOs _levelConfigSOs;
        [SerializeField] private LevelBase _levelPrefab;
        
        [Title("Configs")]
        [SerializeField] private Transform _levelContainer;
        [Space(10)]
        [SerializeField] private bool _isLoop;
        [ShowIf("_isLoop")] private int _startLoopLevelNumber = 1;

        private LevelBase _currentLevel;
        private AsyncOperationHandle<LevelDataSO> _currentLevelHandle;
        private bool _isLevelLoading;
        public int MaxLevel => _levelConfigSOs.Levels.Count;

        public event Action OnLevelLoaded;
        #region LifeCycle Methods

        protected override void Awake()
        {
            base.Awake();
            if (_levelConfigSOs == null)
            {
                Debug.LogError("LevelConfigSOs is not assigned in LevelManager");
                return;
            }
            if (_levelPrefab == null)
            {
                Debug.LogError("LevelPrefab is not assigned in LevelManager");
                return;
            }
            StartLevel().Forget();
        }

        #endregion
        
        #region  Private Methods

        private async UniTask StartLevel()
        {
            int currentLevel = LevelDataBlock.Instance.CurrentLevelNumber;
            LoadLevelByNumber(currentLevel);
        }
        private LevelConfig GetLevelByNumber(int levelNumber)
        {
            foreach (var config in _levelConfigSOs.Levels)
            {
                if (config.LevelNumber == levelNumber) return config;
            }
            Debug.LogError($"Not level with number {levelNumber} found in LevelConfigSOs");
            return null;
        }

        private async UniTask LoadLevelAsync(LevelConfig levelConfig)
        {
            if (_isLevelLoading) return;
            _isLevelLoading = true;
            try
            {
                UnloadCurrentLevel();
                _currentLevelHandle = levelConfig.GameData.LoadAssetAsync<LevelDataSO>();
                await _currentLevelHandle;
                LevelDataSO levelData = _currentLevelHandle.Result;

                // Instantiate the level prefab and initialize it with the loaded level data
                _currentLevel = Instantiate(_levelPrefab, _levelContainer);
                _currentLevel.Initialize(levelData, levelConfig.LevelNumber);

                //Save to Database
                LevelDataBlock.Instance.SetCurrentLevelNumber(levelConfig.LevelNumber);
                
                //Invoke Event
                OnLevelLoaded?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                _isLevelLoading = false;
            }

        }
        private void UnloadCurrentLevel()
        {
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
                _currentLevel = null;
            }

            if (_currentLevelHandle.IsValid())
            {
                Addressables.Release(_currentLevelHandle);
                _currentLevelHandle = default;
            }
        }


        #endregion

        #region Public Methods
        public void ReloadLevel()
        {
            int currentLevelNumber = _currentLevel.LevelNumber;
            if (_currentLevel != null)
            {
                Destroy(_currentLevel.gameObject);
                _currentLevel = null;
            }
            LevelDataSO levelData = _currentLevelHandle.Result;
            
            _currentLevel = Instantiate(_levelPrefab, _levelContainer);
            _currentLevel.Initialize(levelData, currentLevelNumber);
            

        }
        public void LoadNextLevel()
        {
            if (_currentLevel == null)
            {
                Debug.LogError("No current level to load next from.");
                return;
            }
            int currentLevelNumber = _currentLevel.LevelNumber;
            if (currentLevelNumber >= MaxLevel)
            {
                if (!_isLoop)
                {
                    Debug.LogWarning($"Already at the last level ({currentLevelNumber}). Cannot load next level.");
                    return;
                }
                currentLevelNumber = currentLevelNumber % _levelConfigSOs.Levels.Count + _startLoopLevelNumber;

            }
            
            LevelConfig nextLevelConfig = GetLevelByNumber(currentLevelNumber + 1);
            LoadLevelAsync(nextLevelConfig).Forget();
            
        }
        
        public void LoadLevelByNumber(int levelNumber)
        {
            if (levelNumber > MaxLevel)
            {
                if (!_isLoop) return;
                levelNumber = levelNumber % _levelConfigSOs.Levels.Count + _startLoopLevelNumber - 1;
            }
            LevelConfig levelConfig = GetLevelByNumber(levelNumber);
            if (levelConfig == null)
            {
                Debug.LogError($"No level with number {levelNumber} found.");
                return;
            }
            LoadLevelAsync(levelConfig).Forget();
        }
        #endregion
    }
}