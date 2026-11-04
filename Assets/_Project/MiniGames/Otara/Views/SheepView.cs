using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

namespace IntellectualGame.Otara
{
    /// <summary>View of a single sheep: walking with little hops and cheering. Pivot is at the feet.</summary>
    [RequireComponent(typeof(SortingGroup))]
    public class SheepView : MonoBehaviour
    {
        const float WalkSpeed = 3.2f;
        const float HopLength = 0.55f;
        const float HopHeight = 0.12f;

        SortingGroup sortingGroup;
        float scale = 1f;
        float facing = 1f;
        Tween motion;

        void Awake() => sortingGroup = GetComponent<SortingGroup>();

        public void Place(Vector2 position, float newScale)
        {
            scale = newScale;
            transform.position = position;
            Apply(position.y);
        }

        public IEnumerator WalkTo(Vector2 target)
        {
            Vector2 start = transform.position;
            float distance = Vector2.Distance(start, target);
            if (distance > 0.01f)
            {
                if (Mathf.Abs(target.x - start.x) > 0.01f) facing = Mathf.Sign(target.x - start.x);

                motion?.Kill();
                motion = DOVirtual.Float(0f, 1f, distance / WalkSpeed, t =>
                {
                    Vector2 position = Vector2.Lerp(start, target, t);
                    position.y += Mathf.Abs(Mathf.Sin(t * distance / HopLength * Mathf.PI)) * HopHeight;
                    transform.position = position;
                    Apply(position.y);
                }).SetEase(Ease.Linear).SetLink(gameObject);
                yield return motion.WaitForCompletion();
            }

            transform.position = target;
            Apply(target.y);
        }

        public IEnumerator Cheer(int hops)
        {
            Vector2 home = transform.position;
            motion?.Kill();
            motion = transform.DOMoveY(home.y + 0.45f, 0.15f)
                .SetEase(Ease.OutSine)
                .SetLoops(hops * 2, LoopType.Yoyo)
                .SetLink(gameObject);
            yield return motion.WaitForCompletion();
            transform.position = home;
        }

        void Apply(float y)
        {
            transform.localScale = new Vector3(scale * facing, scale, 1f);
            sortingGroup.sortingOrder = 100 + Mathf.RoundToInt(-y * 20f);
        }
    }
}
