using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Yurta
{
    /// <summary>Controller: runs the equations, checks answers and tells the view what to show.</summary>
    public class YurtaGame : Game
    {
        [SerializeField] YurtaLevelData level;
        [SerializeField] YurtaView view;

        YurtaState state;
        bool busy;

        public override string Title => "Юрта: сборка";
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);
        public override string WinMessage => "Юрта готова!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            busy = false;

            if (state == null) state = ScriptableObject.CreateInstance<YurtaState>();
            state.Begin(level.Equations.Count);

            view.ResetView();
            view.SetHudVisible(true);
            ShowCurrentEquation();
        }

        protected override void OnAnswer(int value)
        {
            if (busy) return;

            if (value == level.Equations[state.Index].Answer)
            {
                StartCoroutine(HandleCorrect());
            }
            else
            {
                state.RegisterMistake();
                view.ShowFeedback(false);
            }
        }

        protected override void OnGameStop()
        {
            StopAllCoroutines();
            busy = false;
            view.ResetView();
            view.SetHudVisible(false);
        }

        IEnumerator HandleCorrect()
        {
            busy = true;
            view.ShowFeedback(true);
            yield return view.PlayPartAssembled(state.Index);
            state.Advance();

            if (state.IsComplete)
            {
                view.HideEquation();
                yield return view.PlayTundukReward();
                busy = false;
                Finish(true);
                yield break;
            }

            ShowCurrentEquation();
            busy = false;
        }

        void ShowCurrentEquation() =>
            view.ShowEquation(level.Equations[state.Index].Text, state.Index + 1, state.Total);
    }
}
