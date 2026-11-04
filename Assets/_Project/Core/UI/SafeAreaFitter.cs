using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>Keeps a RectTransform inside <see cref="Screen.safeArea"/> (notches, rounded corners, system bars).</summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        RectTransform rect;
        Rect applied;
        Vector2Int screenSize;

        void Awake() => rect = (RectTransform)transform;

        void OnEnable() => Apply();

        void Update()
        {
            if (Screen.safeArea != applied || screenSize.x != Screen.width || screenSize.y != Screen.height) Apply();
        }

        void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0) return;

            applied = Screen.safeArea;
            screenSize = new Vector2Int(Screen.width, Screen.height);

            rect.anchorMin = new Vector2(applied.xMin / Screen.width, applied.yMin / Screen.height);
            rect.anchorMax = new Vector2(applied.xMax / Screen.width, applied.yMax / Screen.height);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
