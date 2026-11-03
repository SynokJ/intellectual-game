using System;
using System.Collections;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>Tiny coroutine-based animation helpers shared by the views.</summary>
    public static class Tween
    {
        /// <summary>Calls <paramref name="apply"/> with a value going from 0 to 1 over <paramref name="duration"/> seconds.</summary>
        public static IEnumerator Animate(float duration, Action<float> apply)
        {
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                apply(elapsed / duration);
                yield return null;
            }
            apply(1f);
        }

        public static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }

        public static float EaseOutBounce(float t)
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
