using System.Collections;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Shyrdak
{
    /// <summary>Controller: the player paints the carpet part by part, choosing the palette number of the sample color.</summary>
    public class ShyrdakGame : Game
    {
        [SerializeField] ShyrdakLevelData level;
        [SerializeField] ShyrdakView view;
        [SerializeField] GameHudView hud;

        ShyrdakState state;
        bool awaitingAnswer;

        public override string Title => "Шырдак";
        public override int AnswerCount => Mathf.Max(level.MaxAnswer, MinAnswerCount);
        public override string WinMessage => "Шырдак готов!";

        protected override void OnGameStart()
        {
            StopAllCoroutines();
            if (state == null) state = ScriptableObject.CreateInstance<ShyrdakState>();
            var run = level.CreateRun();
            state.Begin(run);

            view.ResetView();
            hud.ResetView();
            view.SetVisible(true);
            hud.SetVisible(true);
            view.ShowSample(level.Palette, run);
            ShowPart();
        }

        protected override void OnAnswer(int number)
        {
            if (!awaitingAnswer) return;

            if (number - 1 == state.Current)
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
            view.ResetView();
            hud.ResetView();
            view.SetVisible(false);
            hud.SetVisible(false);
        }

        void ShowPart()
        {
            view.Highlight(state.Index);
            hud.ShowProgress($"Деталь {state.Index + 1} из {state.Total}");
            hud.ShowQuestion("Покрась мигающую деталь так же, как на образце.\nВыбери номер цвета");
            awaitingAnswer = true;
        }

        IEnumerator HandleCorrect()
        {
            hud.ShowFeedback(true);
            yield return view.PlayPaint(state.Index, level.Palette[state.Current]);
            state.Advance();

            if (state.IsComplete)
            {
                hud.HideQuestion();
                yield return view.PlayFinale();
                Finish(true);
                yield break;
            }

            ShowPart();
        }
    }
}
