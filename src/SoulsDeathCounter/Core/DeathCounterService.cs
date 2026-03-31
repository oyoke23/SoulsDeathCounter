using System;
using SoulsDeathCounter.Models;

namespace SoulsDeathCounter.Core
{
    public class DeathCounterService : IDisposable
    {
        private readonly GameDetector _gameDetector;
        private readonly MemoryReader _memoryReader;

        private DetectedGame _currentGame;
        private int _lastDeathCount;
        private bool _disposed;

        public event Action<int> DeathCountChanged;
        public event Action<string> GameDetected;
        public event Action GameLost;

        public string CurrentGameName => _currentGame?.Definition?.Name;
        public bool IsGameRunning => _currentGame != null && !_currentGame.Process.HasExited;
        public int CurrentDeathCount => _lastDeathCount;

        public DeathCounterService()
        {
            _gameDetector = new GameDetector();
            _memoryReader = new MemoryReader();
        }

        public void Update()
        {
            if (_currentGame == null || _currentGame.Process.HasExited)
            {
                TryDetectNewGame();
                return;
            }

            var deathCount = ReadDeathCount();

            if (deathCount != _lastDeathCount)
            {
                _lastDeathCount = deathCount;
                DeathCountChanged?.Invoke(deathCount);
            }
        }

        private void TryDetectNewGame()
        {
            if (_currentGame != null)
            {
                _memoryReader.Detach();
                _currentGame = null;
                _lastDeathCount = 0;
                GameLost?.Invoke();
            }

            var detected = _gameDetector.TryDetectGame();

            if (detected != null && _memoryReader.Attach(detected.Process))
            {
                _currentGame = detected;
                _lastDeathCount = ReadDeathCount();
                GameDetected?.Invoke(detected.Definition.Name);
                DeathCountChanged?.Invoke(_lastDeathCount);
            }
        }

        private int ReadDeathCount()
        {
            if (_currentGame == null)
                return 0;

            try
            {
                var finalAddress = _memoryReader.FollowPointerChain(
                    _currentGame.DeathCounterBaseAddress,
                    _currentGame.Definition.PointerOffsets
                );

                if (finalAddress == IntPtr.Zero)
                    return _lastDeathCount;

                var count = _memoryReader.ReadInt32(finalAddress);

                if (count < 0 || count > 100000)
                    return _lastDeathCount;

                return count;
            }
            catch
            {
                return _lastDeathCount;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _memoryReader?.Dispose();
                _disposed = true;
            }
        }
    }
}
