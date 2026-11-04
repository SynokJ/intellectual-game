using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Bazaar
{
    /// <summary>Controller: shows each shopping problem on the stage, waits for the answer and checks it.</summary>
    public class BazaarGame : Game
    {
        [SerializeField] BazaarLevelData level;
        [SerializeField] BazaarStageView stage;
        [SerializeField] BazaarHudView hud;

        BazaarState state;
        bool awaitingAnswer;

        public override string Title => "Базар и бешбармак";
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);
        public override string WinMessage => "Бешбармак подан гостям!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            awaitingAnswer = false;

            if (state == null) state = ScriptableObject.CreateInstance<BazaarState>();
            state.Begin(level.CreateRun());

            stage.ResetView();
            hud.ResetView();
            stage.SetVisible(true);
            hud.SetVisible(true);
            StartCoroutine(PlayRound());
        }

        protected override void OnAnswer(int value)
        {
            if (!awaitingAnswer) return;

            if (value == state.Current.Answer)
            {
                awaitingAnswer = false;
                StartCoroutine(HandleCorrect());
            }
            else
            {
                state.RegisterMistake();
                hud.ShowFeedback(false);
            }
        }

        protected override void OnGameStop()
        {
            StopAllCoroutines();
            awaitingAnswer = false;
            stage.ResetView();
            hud.ResetView();
            stage.SetVisible(false);
            hud.SetVisible(false);
        }

        IEnumerator PlayRound()
        {
            var round = state.Current;
            hud.ShowProgress(state.Index + 1, state.Total);
            yield return stage.PlayIntro(round, level.Products[round.productIndex]);

            hud.ShowQuestion(level.Question(round));
            awaitingAnswer = true;
        }

        IEnumerator HandleCorrect()
        {
            var round = state.Current;
            hud.ShowFeedback(true);
            yield return stage.PlayCorrect(round, level.Products[round.productIndex]);
            state.Advance();
            hud.HideQuestion();

            if (state.IsComplete)
            {
                yield return stage.PlayFinale();
                Finish(true);
                yield break;
            }

            yield return PlayRound();
        }
    }
}
