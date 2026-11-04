using System;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Base class of every mini-game. A mini-game is a self-contained prefab (world, HUD, answer pad, controller)
    /// that the GameController spawns when it is chosen and destroys when the player leaves.
    /// Holds the abstract flow (start, answer, stop, finish); concrete games (Controller role of MVC) implement the rules.
    /// </summary>
    public abstract class Game : MonoBehaviour
    {
        protected const int MinAnswerCount = 9;

        [SerializeField] AnswerPadView answerPad;

        public event Action<Game, bool> Finished;

        public bool IsRunning { get; private set; }

        public abstract string Title { get; }
        public virtual string WinMessage => "Победа!";

        /// <summary>How many answer buttons (numbered from 1) the pad of this game shows.</summary>
        public virtual int AnswerCount => 9;

        public void StartGame()
        {
            if (IsRunning) return;
            IsRunning = true;

            answerPad.AnswerPressed -= SubmitAnswer;
            answerPad.AnswerPressed += SubmitAnswer;
            answerPad.Show(AnswerCount);
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
            answerPad.AnswerPressed -= SubmitAnswer;
            answerPad.Hide();
            OnGameStop();
        }

        protected void Finish(bool won)
        {
            if (!IsRunning) return;
            IsRunning = false;
            answerPad.AnswerPressed -= SubmitAnswer;
            Finished?.Invoke(this, won);
        }

        protected abstract void OnGameStart();
        protected abstract void OnAnswer(int value);
        protected abstract void OnGameStop();
    }
}
