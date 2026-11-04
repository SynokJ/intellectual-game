using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace IntellectualGame.Bazaar
{
    /// <summary>View of one product or coin: a body shape with a lighter highlight on top. Pivot is its center.</summary>
    [RequireComponent(typeof(SortingGroup))]
    public class ItemView : MonoBehaviour
    {
        const float HighlightMix = 0.45f;

        [SerializeField] SpriteRenderer body;
        [SerializeField] SpriteRenderer highlight;

        SortingGroup group;

        SortingGroup Group => group != null ? group : (group = GetComponent<SortingGroup>());

        public void Style(Sprite sprite, Color color, float size)
        {
            body.sprite = sprite;
            body.color = color;
            highlight.sprite = sprite;
            highlight.color = Color.Lerp(color, Color.white, HighlightMix);
            transform.localScale = Vector3.one * size;
        }

        public void SetOrder(int order) => Group.sortingOrder = order;

        public Tween PopIn(float delay)
        {
            Vector3 size = transform.localScale;
            transform.localScale = Vector3.zero;
            return transform.DOScale(size, 0.3f).SetEase(Ease.OutBack).SetDelay(delay).SetLink(gameObject);
        }

        public Tween JumpTo(Vector3 target, float delay)
        {
            return transform.DOJump(target, 0.8f, 1, 0.5f).SetDelay(delay).SetLink(gameObject);
        }

        public Tween Shrink(float delay)
        {
            return transform.DOScale(0f, 0.15f).SetDelay(delay).SetLink(gameObject);
        }

        public Tween Pulse(float delay)
        {
            return transform.DOPunchScale(transform.localScale * 0.45f, 0.5f, 4).SetDelay(delay).SetLink(gameObject);
        }
    }
}
