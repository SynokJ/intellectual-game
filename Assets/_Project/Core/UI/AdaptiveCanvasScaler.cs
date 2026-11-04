using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>Switches the canvas reference resolution between landscape (PC, phone on its side) and portrait (phone).</summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveCanvasScaler : MonoBehaviour
    {
        [SerializeField] Vector2 landscapeReference = new Vector2(1920f, 1080f);
        [SerializeField] Vector2 portraitReference = new Vector2(1080f, 1920f);
        [SerializeField, Range(0f, 1f)] float match = 0.5f;

        CanvasScaler scaler;
        bool portrait;

        void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            Apply(IsPortrait);
        }

        void Update()
        {
            if (IsPortrait != portrait) Apply(IsPortrait);
        }

        static bool IsPortrait => Screen.height > Screen.width;

        void Apply(bool isPortrait)
        {
            portrait = isPortrait;
            scaler.referenceResolution = isPortrait ? portraitReference : landscapeReference;
            scaler.matchWidthOrHeight = match;
        }
    }
}
