using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Alchik
{
    /// <summary>Controller: the player picks a force, the physics decides where the knucklebone lands.</summary>
    public class AlchikGame : Game
    {
        [SerializeField] AlchikLevelData level;
        [SerializeField] AlchikView view;
        [SerializeField] GameHudView hud;

        AlchikState state;
        bool awaitingThrow;

        public override string Title => "Альчик";
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);
        public override string WinMessage => "Все альчики попали в цель!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            if (state == null) state = ScriptableObject.CreateInstance<AlchikState>();
            state.Begin(level.CreateRun());

            view.ResetView();
            hud.ResetView();
            view.SetVisible(true);
            hud.SetVisible(true);
            ShowRound();
        }

        protected override void OnAnswer(int force)
        {
            if (!awaitingThrow) return;
            awaitingThrow = false;
            StartCoroutine(Throw(force));
        }

        protected override void OnGameStop()
        {
            StopAllCoroutines();
            awaitingThrow = false;
            view.ResetView();
            hud.ResetView();
            view.SetVisible(false);
            hud.SetVisible(false);
        }

        void ShowRound()
        {
            var round = state.Current;
            view.ShowRound(round, level.HitRadius);
            hud.ShowProgress($"Бросок {state.Index + 1} из {state.Total}");
            hud.ShowQuestion($"Угол броска {Mathf.RoundToInt(round.angle)}°.\nВыбери силу от 1 до 9, чтобы попасть в кон");
            awaitingThrow = true;
        }

        IEnumerator Throw(int force)
        {
            var round = state.Current;
            float landing = level.Range(force, round.angle);
            yield return view.PlayThrow(landing, level.Apex(force, round.angle), level.FlightTime(force, round.angle));

            if (!level.IsHit(round, force))
            {
                state.RegisterMistake();
                hud.ShowFeedback(false);
                awaitingThrow = true;
                yield break;
            }

            hud.ShowFeedback(true);
            yield return view.PlayHit();
            state.Advance();

            if (state.IsComplete)
            {
                hud.HideQuestion();
                yield return view.PlayFinale();
                Finish(true);
                yield break;
            }

            ShowRound();
        }
    }
}
