using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>Grows the orthographic size on tall (portrait) screens so the whole width of the game world stays visible.</summary>
    [RequireComponent(typeof(Camera))]
    public class CameraFit : MonoBehaviour
    {
        [SerializeField] float minHeight = 10f;
        [SerializeField] float minWidth = 16f;

        Camera cam;
        Vector2Int screenSize;

        void Awake() => cam = GetComponent<Camera>();

        void Update()
        {
            if (screenSize.x != Screen.width || screenSize.y != Screen.height) Apply();
        }

        void Apply()
        {
            screenSize = new Vector2Int(Screen.width, Screen.height);
            if (screenSize.y <= 0) return;

            float aspect = (float)screenSize.x / screenSize.y;
            cam.orthographicSize = Mathf.Max(minHeight / 2f, minWidth / (2f * aspect));
        }
    }
}
