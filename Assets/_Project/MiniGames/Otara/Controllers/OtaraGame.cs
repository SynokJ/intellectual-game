using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Otara
{
    /// <summary>Controller: plays each round's scenario, then waits for the answer and checks it.</summary>
    public class OtaraGame : Game
    {
        [SerializeField] OtaraLevelData level;
        [SerializeField] OtaraView view;

        OtaraState state;
        bool awaitingAnswer;

        public override string Title => "Отара овец";
        public override string WinMessage => "Отара в безопасности!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            awaitingAnswer = false;

            if (state == null) state = ScriptableObject.CreateInstance<OtaraState>();
            state.Begin(level.Rounds.Count);

            view.ResetView();
            view.SetVisible(true);
            StartCoroutine(PlayRound());
        }

        protected override void OnAnswer(int value)
        {
            if (!awaitingAnswer) return;

            if (value == level.Rounds[state.Index].Answer)
            {
                awaitingAnswer = false;
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
            awaitingAnswer = false;
            view.ResetView();
            view.SetVisible(false);
        }

        IEnumerator PlayRound()
        {
            var round = level.Rounds[state.Index];
            view.PrepareRound(round, state.Index + 1, state.Total);
            yield return new WaitForSeconds(0.8f);

            if (round.kind == OtaraRoundKind.Count)
            {
                foreach (int move in round.moves)
                {
                    yield return view.PlayMove(move);
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                yield return view.PlayShare(round.families);
            }

            view.ShowEvent(string.Empty);
            view.ShowQuestion(round.Question);
            awaitingAnswer = true;
        }

        IEnumerator HandleCorrect()
        {
            view.ShowFeedback(true);
            yield return view.PlayCelebrate();
            state.Advance();

            if (state.IsComplete)
            {
                view.HideQuestion();
                yield return new WaitForSeconds(0.4f);
                Finish(true);
                yield break;
            }

            view.HideQuestion();
            yield return PlayRound();
        }
    }
}
