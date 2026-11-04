using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Kerege
{
    /// <summary>Controller: the player presses the numbers of the tiles in the order of the letters of the word.</summary>
    public class KeregeGame : Game
    {
        [SerializeField] KeregeLevelData level;
        [SerializeField] KeregeView view;
        [SerializeField] GameHudView hud;

        KeregeState state;
        bool[] used;
        int placed;
        bool awaitingAnswer;

        public override string Title => "Кереге-слова";
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);
        public override string WinMessage => "Все слова собраны!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            if (state == null) state = ScriptableObject.CreateInstance<KeregeState>();
            state.Begin(level.CreateRun());

            view.ResetView();
            hud.ResetView();
            view.SetVisible(true);
            hud.SetVisible(true);
            ShowRound();
        }

        protected override void OnAnswer(int number)
        {
            if (!awaitingAnswer) return;

            var round = state.Current;
            int tile = number - 1;
            bool valid = tile >= 0 && tile < round.tiles.Length && !used[tile];

            if (valid && round.tiles[tile] == round.word[placed])
            {
                used[tile] = true;
                view.PlaceLetter(tile, placed);
                placed++;
                if (placed == round.word.Length)
                {
                    awaitingAnswer = false;
                    StartCoroutine(HandleWordDone());
                }
            }
            else
            {
                state.RegisterMistake();
                view.ShakeTile(tile);
                hud.ShowFeedback(false);
            }
        }

        protected override void OnGameStop()
        {
            StopAllCoroutines();
            awaitingAnswer = false;
            view.ResetView();
            hud.ResetView();
            view.SetVisible(false);
            hud.SetVisible(false);
        }

        void ShowRound()
        {
            var round = state.Current;
            used = new bool[round.tiles.Length];
            placed = 0;
            view.ShowRound(round);
            hud.ShowProgress($"Слово {state.Index + 1} из {state.Total}");
            hud.ShowQuestion($"Собери слово: «{round.hint}».\nНажимай номера плиток по порядку букв");
            awaitingAnswer = true;
        }

        IEnumerator HandleWordDone()
        {
            hud.ShowFeedback(true);
            yield return view.PlayWordDone();
            state.Advance();

            if (state.IsComplete)
            {
                hud.HideQuestion();
                Finish(true);
                yield break;
            }

            ShowRound();
        }
    }
}
