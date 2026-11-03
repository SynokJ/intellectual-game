using System.Collections;
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

                float duration = distance / WalkSpeed;
                for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
                {
                    float t = elapsed / duration;
                    Vector2 position = Vector2.Lerp(start, target, t);
                    position.y += Mathf.Abs(Mathf.Sin(t * distance / HopLength * Mathf.PI)) * HopHeight;
                    transform.position = position;
                    Apply(position.y);
                    yield return null;
                }
            }

            transform.position = target;
            Apply(target.y);
        }

        public IEnumerator Cheer(int hops)
        {
            Vector2 home = transform.position;
            for (int i = 0; i < hops; i++)
            {
                for (float elapsed = 0f; elapsed < 0.3f; elapsed += Time.deltaTime)
                {
                    transform.position = home + Vector2.up * (Mathf.Sin(elapsed / 0.3f * Mathf.PI) * 0.45f);
                    yield return null;
                }
            }
            transform.position = home;
        }

        void Apply(float y)
        {
            transform.localScale = new Vector3(scale * facing, scale, 1f);
            sortingGroup.sortingOrder = 100 + Mathf.RoundToInt(-y * 20f);
        }
    }
}
