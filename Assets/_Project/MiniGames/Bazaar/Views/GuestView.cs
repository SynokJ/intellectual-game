using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace IntellectualGame.Bazaar
{
    /// <summary>View of a person (guest or merchant): robe, head and a tebetey hat. Pivot is at the feet.</summary>
    [RequireComponent(typeof(SortingGroup))]
    public class GuestView : MonoBehaviour
    {
        [SerializeField] SpriteRenderer robe;
        [SerializeField] SpriteRenderer hat;

        SortingGroup group;

        SortingGroup Group => group != null ? group : (group = GetComponent<SortingGroup>());

        public void Dress(Color robeColor, Color hatColor)
        {
            robe.color = robeColor;
            hat.color = hatColor;
        }

        public void SetOrder(int order) => Group.sortingOrder = order;

        public Tween PopIn(float delay)
        {
            Vector3 size = transform.localScale;
            transform.localScale = Vector3.zero;
            return transform.DOScale(size, 0.4f).SetEase(Ease.OutBack).SetDelay(delay).SetLink(gameObject);
        }

        public Tween Cheer(float delay)
        {
            return transform.DOPunchPosition(Vector3.up * 0.5f, 0.6f, 1, 0f).SetDelay(delay).SetLink(gameObject);
        }
    }
}
