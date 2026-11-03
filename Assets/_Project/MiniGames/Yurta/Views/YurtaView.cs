using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Yurta
{
    /// <summary>View: shows the equation, plays the assembly animations. Knows nothing about the rules.</summary>
    public class YurtaView : MonoBehaviour
    {
        [SerializeField] GameObject hudRoot;
        [SerializeField] Text equationText;
        [SerializeField] Text progressText;
        [SerializeField] Text feedbackText;
        [SerializeField] Transform[] parts;
        [SerializeField] Transform tunduk;

        static readonly Color CorrectColor = new Color(0.15f, 0.55f, 0.2f);
        static readonly Color WrongColor = new Color(0.75f, 0.15f, 0.15f);

        Vector3 tundukHome;
        Vector2 equationHome;
        Coroutine feedbackRoutine;

        void Awake()
        {
            tundukHome = tunduk.position;
            equationHome = equationText.rectTransform.anchoredPosition;
        }

        public void ResetView()
        {
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = null;

            foreach (var part in parts) part.gameObject.SetActive(false);
            tunduk.gameObject.SetActive(false);
            tunduk.position = tundukHome;
            tunduk.rotation = Quaternion.identity;
            tunduk.localScale = Vector3.one;

            equationText.rectTransform.anchoredPosition = equationHome;
            equationText.rectTransform.localScale = Vector3.one;
            equationText.gameObject.SetActive(true);
            progressText.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        public void SetHudVisible(bool visible) => hudRoot.SetActive(visible);

        public void ShowEquation(string text, int number, int total)
        {
            equationText.gameObject.SetActive(true);
            equationText.text = text;
            progressText.text = $"Шаг {number} из {total}";
        }

        public void HideEquation()
        {
            equationText.gameObject.SetActive(false);
            progressText.text = string.Empty;
            feedbackText.text = string.Empty;
        }

        public void ShowFeedback(bool correct)
        {
            if (feedbackRoutine != null) StopCoroutine(feedbackRoutine);
            feedbackRoutine = StartCoroutine(FeedbackRoutine(correct));
        }

        public IEnumerator PlayPartAssembled(int partIndex)
        {
            var part = parts[Mathf.Clamp(partIndex, 0, parts.Length - 1)];
            part.gameObject.SetActive(true);
            yield return Animate(0.5f, t => part.localScale = Vector3.one * EaseOutBack(t));
            part.localScale = Vector3.one;
        }

        public IEnumerator PlayTundukReward()
        {
            tunduk.gameObject.SetActive(true);
            Vector3 start = tundukHome + Vector3.up * 4f;
            yield return Animate(1.1f, t =>
            {
                tunduk.position = Vector3.LerpUnclamped(start, tundukHome, EaseOutBounce(t));
                tunduk.rotation = Quaternion.Euler(0f, 0f, 360f * (1f - t));
            });
            tunduk.position = tundukHome;
            tunduk.rotation = Quaternion.identity;

            yield return Animate(0.4f, t =>
                tunduk.localScale = Vector3.one * (1f + 0.25f * Mathf.Sin(t * Mathf.PI)));
            tunduk.localScale = Vector3.one;
            yield return new WaitForSeconds(0.6f);
        }

        IEnumerator FeedbackRoutine(bool correct)
        {
            feedbackText.color = correct ? CorrectColor : WrongColor;
            feedbackText.text = correct ? "Верно!" : "Неверно, попробуй ещё";

            var rect = equationText.rectTransform;
            if (correct)
            {
                yield return Animate(0.3f, t => rect.localScale = Vector3.one * (1f + 0.2f * Mathf.Sin(t * Mathf.PI)));
                rect.localScale = Vector3.one;
            }
            else
            {
                yield return Animate(0.35f, t =>
                    rect.anchoredPosition = equationHome + Vector2.right * (Mathf.Sin(t * 40f) * 18f * (1f - t)));
                rect.anchoredPosition = equationHome;
            }

            yield return new WaitForSeconds(0.6f);
            feedbackText.text = string.Empty;
            feedbackRoutine = null;
        }

        static IEnumerator Animate(float duration, Action<float> apply)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                apply(elapsed / duration);
                yield return null;
            }
            apply(1f);
        }

        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        static float EaseOutBounce(float t)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (t < 1f / d1) return n1 * t * t;
            if (t < 2f / d1) { t -= 1.5f / d1; return n1 * t * t + 0.75f; }
            if (t < 2.5f / d1) { t -= 2.25f / d1; return n1 * t * t + 0.9375f; }
            t -= 2.625f / d1;
            return n1 * t * t + 0.984375f;
        }
    }
}
