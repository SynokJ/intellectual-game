using System.Collections;
using DG.Tweening;
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

        static readonly Color CorrectColor = new Color(0.5f, 0.95f, 0.55f);
        static readonly Color WrongColor = new Color(1f, 0.55f, 0.5f);

        Vector3 tundukHome;
        Vector2 equationHome;
        Sequence feedbackSequence;
        Sequence tundukSequence;

        void Awake()
        {
            tundukHome = tunduk.position;
            equationHome = equationText.rectTransform.anchoredPosition;
        }

        public void ResetView()
        {
            StopFeedback();
            tundukSequence?.Kill();
            tundukSequence = null;

            foreach (var part in parts)
            {
                part.DOKill();
                part.gameObject.SetActive(false);
            }
            tunduk.gameObject.SetActive(false);
            tunduk.position = tundukHome;
            tunduk.rotation = Quaternion.identity;
            tunduk.localScale = Vector3.one;

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
            StopFeedback();
            feedbackText.color = correct ? CorrectColor : WrongColor;
            feedbackText.text = correct ? "Верно!" : "Неверно, попробуй ещё";

            var rect = equationText.rectTransform;
            feedbackSequence = DOTween.Sequence().SetLink(gameObject);
            if (correct)
            {
                feedbackSequence
                    .Append(rect.DOScale(1.2f, 0.15f).SetEase(Ease.OutSine))
                    .Append(rect.DOScale(1f, 0.15f).SetEase(Ease.InSine));
            }
            else
            {
                feedbackSequence.Append(rect.DOShakeAnchorPos(0.35f, new Vector2(18f, 0f), 13, 0f));
            }

            feedbackSequence.AppendInterval(0.6f).OnComplete(() =>
            {
                feedbackText.text = string.Empty;
                feedbackSequence = null;
            });
        }

        public IEnumerator PlayPartAssembled(int partIndex)
        {
            var part = parts[Mathf.Clamp(partIndex, 0, parts.Length - 1)];
            part.DOKill();
            part.gameObject.SetActive(true);
            part.localScale = Vector3.zero;
            yield return part.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetLink(part.gameObject).WaitForCompletion();
        }

        public IEnumerator PlayTundukReward()
        {
            tundukSequence?.Kill();
            tunduk.gameObject.SetActive(true);
            tunduk.position = tundukHome + Vector3.up * 4f;
            tunduk.rotation = Quaternion.identity;
            tunduk.localScale = Vector3.one;

            tundukSequence = DOTween.Sequence().SetLink(gameObject)
                .Append(tunduk.DOMove(tundukHome, 1.1f).SetEase(Ease.OutBounce))
                .Join(tunduk.DORotate(new Vector3(0f, 0f, -360f), 1.1f, RotateMode.FastBeyond360).SetEase(Ease.Linear))
                .Append(tunduk.DOScale(1.25f, 0.2f).SetEase(Ease.OutSine))
                .Append(tunduk.DOScale(1f, 0.2f).SetEase(Ease.InSine))
                .AppendInterval(0.6f);
            yield return tundukSequence.WaitForCompletion();
        }

        void StopFeedback()
        {
            feedbackSequence?.Kill();
            feedbackSequence = null;

            var rect = equationText.rectTransform;
            rect.localScale = Vector3.one;
            rect.anchoredPosition = equationHome;
        }
    }
}
