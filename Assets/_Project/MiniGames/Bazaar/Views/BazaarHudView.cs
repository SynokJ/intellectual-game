using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Bazaar
{
    /// <summary>View: the question, the round counter and the right/wrong feedback. Knows nothing about the rules.</summary>
    public class BazaarHudView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Text questionText;
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
            feedbackText.text = string.Empty;
            progressText.text = string.Empty;
        }

        public void SetVisible(bool visible) => root.SetActive(visible);

        public void ShowProgress(int number, int total) => progressText.text = $"Раунд {number} из {total}";

        public void ShowQuestion(string text) => questionText.text = text;

        public void HideQuestion()
        {
            questionText.text = string.Empty;
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
