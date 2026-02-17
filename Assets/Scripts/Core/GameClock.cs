using UnityEngine;
using VContainer.Unity;

namespace Hostage.Core
{
    public class GameClock : ITickable
    {
        const float SecondsPerDay = 86400f;

        float _currentTimeInSeconds;
        float _timeMultiplier;

        public GameClock()
        {
            _currentTimeInSeconds = 0f;
            _timeMultiplier = 1f;
        }

        public bool Paused { get; set; }
        public float GameDeltaTime { get; private set; }
        public float TimeMultiplier
        {
            get => _timeMultiplier;
            set => _timeMultiplier = value;
        }

        public int CurrentHour => (int)(_currentTimeInSeconds / 3600f) % 24;
        public int CurrentMinute => (int)(_currentTimeInSeconds % 3600f / 60f);

        public string FormattedTime
        {
            get
            {
                int hour = CurrentHour;
                string period = hour >= 12 ? "PM" : "AM";
                int displayHour = hour % 12;
                if (displayHour == 0) displayHour = 12;
                return $"{displayHour}:{CurrentMinute:D2} {period}";
            }
        }

        public void SetTime(float hour, float minute)
        {
            _currentTimeInSeconds = hour * 3600f + minute * 60f;
        }

        public void Tick()
        {
            if (Paused)
            {
                GameDeltaTime = 0f;
                return;
            }

            GameDeltaTime = Time.deltaTime * _timeMultiplier;
            _currentTimeInSeconds += GameDeltaTime;
            if (_currentTimeInSeconds >= SecondsPerDay)
                _currentTimeInSeconds -= SecondsPerDay;
        }
    }
}
