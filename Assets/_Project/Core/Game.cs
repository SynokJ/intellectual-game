using System;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Base class of every mini-game. Holds the abstract flow (start, answer, stop, finish);
    /// concrete games (Controller role of MVC) implement the rules.
    /// </summary>
    public abstract class Game : MonoBehaviour
    {
        [Tooltip("Number button (1-based) that starts this game from the menu.")]
        [SerializeField] int menuNumber = 1;

        public event Action<Game, bool> Finished;

        public int MenuNumber => menuNumber;

        public bool IsRunning { get; private set; }

        public abstract string Title { get; }
        public virtual string WinMessage => "Победа!";

        public void StartGame()
        {
            if (IsRunning) return;
            IsRunning = true;
            OnGameStart();
        }

        public void SubmitAnswer(int value)
        {
            if (!IsRunning) return;
            OnAnswer(value);
        }

        /// <summary>Aborts the game and resets its view. Safe to call when the game is not running.</summary>
        public void StopGame()
        {
            IsRunning = false;
            OnGameStop();
        }

        protected void Finish(bool won)
        {
            if (!IsRunning) return;
            IsRunning = false;
            Finished?.Invoke(this, won);
        }

        protected abstract void OnGameStart();
        protected abstract void OnAnswer(int value);
        protected abstract void OnGameStop();
    }
}
