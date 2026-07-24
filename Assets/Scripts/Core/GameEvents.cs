using System;

namespace Countdown.Core
{
    public static class GameEvents
    {
        public static event Action OnSymptomRevealed;
        public static event Action OnShortlistChanged;
        public static event Action OnGameOver;
        public static event Action OnGameWon;

        public static void RaiseSymptomRevealed() => OnSymptomRevealed?.Invoke();
        public static void RaiseShortlistChanged() => OnShortlistChanged?.Invoke();
        public static void RaiseGameOver() => OnGameOver?.Invoke();
        public static void RaiseGameWon() => OnGameWon?.Invoke();
    }
}
