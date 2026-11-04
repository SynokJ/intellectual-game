using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>View: the question, answer options, round counter and right/wrong feedback of a game. Knows no rules.</summary>
    public class GameHudView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Text questionText;
        [SerializeField] Text optionsText;
        [SerializeField] Text feedbackText;
        [SerializeField] Text progressText;

        static readonly Color CorrectColor = new Color(0.5f, 0.95f, 0.55f);
        static readonly Color WrongColor = new Color(1f, 0.55f, 0.5f);

        Vector2 questionHome;
        Sequence feedbackSequence;

        void Awake() => questionHome = questionText.rectTransform.anchoredPosition;

        public void ResetView()
        {
            StopFeedback();
            questionText.text = string.Empty;
            optionsText.text = string.Empty;
            feedbackText.text = string.Empty;
            progressText.text = string.Empty;
        }

        public void SetVisible(bool visible) => root.SetActive(visible);

        public void ShowProgress(string text) => progressText.text = text;

        public void ShowQuestion(string text) => questionText.text = text;

        /// <summary>Free text under the question, e.g. the explanation of a proverb.</summary>
        public void ShowDetails(string text) => optionsText.text = text;

        public void ShowOptions(IReadOnlyList<string> options)
        {
            var lines = new string[options.Count];
            for (int i = 0; i < lines.Length; i++) lines[i] = $"{i + 1}.  {options[i]}";
            optionsText.text = string.Join("\n", lines);
        }

        public void HideQuestion()
        {
            questionText.text = string.Empty;
            optionsText.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        public void ShowFeedback(bool correct)
        {
            StopFeedback();
            feedbackText.color = correct ? CorrectColor : WrongColor;
            feedbackText.text = correct ? "Верно!" : "Неверно, попробуй ещё";

            feedbackSequence = DOTween.Sequence().SetLink(gameObject);
            if (!correct)
                feedbackSequence.Append(questionText.rectTransform.DOShakeAnchorPos(0.35f, new Vector2(18f, 0f), 13, 0f));
            feedbackSequence.AppendInterval(0.7f).OnComplete(() =>
            {
                feedbackText.text = string.Empty;
                feedbackSequence = null;
            });
        }

        void StopFeedback()
        {
            feedbackSequence?.Kill();
            feedbackSequence = null;
            questionText.rectTransform.anchoredPosition = questionHome;
        }
    }
}
