using System.Collections;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>Controller of every multiple-choice game: asks the questions, checks the chosen option number, moves the stage.</summary>
    public class QuizGame : Game
    {
        const float NoteSeconds = 1.8f;

        [SerializeField] QuizLevelData level;
        [SerializeField] GameHudView hud;
        [SerializeField] QuizStageView stage;

        QuizState state;
        bool awaitingAnswer;

        public override string Title => level.Title;
        public override string WinMessage => level.WinMessage;
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            awaitingAnswer = false;

            if (state == null) state = ScriptableObject.CreateInstance<QuizState>();
            state.Begin(level.CreateRun());

            stage.ResetView();
            hud.ResetView();
            stage.SetVisible(true);
            hud.SetVisible(true);
            stage.Begin(state.Total);
            ShowRound();
        }

        protected override void OnAnswer(int value)
        {
            if (!awaitingAnswer) return;

            if (value == state.Current.correctNumber)
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

        void ShowRound()
        {
            var round = state.Current;
            hud.ShowProgress($"Вопрос {state.Index + 1} из {state.Total}");
            hud.ShowQuestion(round.text);
            hud.ShowOptions(round.options);
            awaitingAnswer = true;
        }

        IEnumerator HandleCorrect()
        {
            var round = state.Current;
            hud.ShowFeedback(true);
            if (!string.IsNullOrEmpty(round.note)) hud.ShowDetails(round.note);

            yield return stage.PlayCorrect(state.Index, state.Total);
            if (!string.IsNullOrEmpty(round.note)) yield return new WaitForSeconds(NoteSeconds);

            state.Advance();
            hud.HideQuestion();

            if (state.IsComplete)
            {
                yield return stage.PlayFinale();
                Finish(true);
                yield break;
            }

            ShowRound();
        }
    }
}
