using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Kerege
{
    /// <summary>View of one letter tile (with its answer-pad number) or one slot of the word being built.</summary>
    public class LetterTileView : MonoBehaviour
    {
        const float UsedAlpha = 0.25f;

        [SerializeField] Text letterText;
        [SerializeField] Text numberText;
        [SerializeField] CanvasGroup group;

        public void Set(string letter, string number)
        {
            letterText.text = letter;
            numberText.text = number;
        }

        public string Letter => letterText.text;

        public void SetLetter(string letter) => letterText.text = letter;

        public void SetUsed() => group.alpha = UsedAlpha;

        public Tween Pop()
        {
            transform.DOKill(true);
            return transform.DOPunchScale(Vector3.one * 0.3f, 0.3f).SetLink(gameObject);
        }

        public Tween Shake()
        {
            transform.DOKill(true);
            return transform.DOShakePosition(0.3f, new Vector3(12f, 0f, 0f), 12, 0f).SetLink(gameObject);
        }
    }
}
