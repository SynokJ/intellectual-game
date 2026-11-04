using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Draws a transparent black plate right behind a Text so it stays readable on any background.
    /// The plate follows the text (size, shake, scale) and hides together with it or while it is empty.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextBackdrop : MonoBehaviour
    {
        [SerializeField] Sprite sprite;
        [SerializeField] Color color = new Color(0f, 0f, 0f, 0.6f);
        [SerializeField] Vector2 padding = new Vector2(40f, 16f);

        Text text;
        RectTransform rect;
        RectTransform plate;

        void Awake()
        {
            text = GetComponent<Text>();
            rect = (RectTransform)transform;

            var go = new GameObject(name + " Backdrop", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(rect.parent, false);
            go.transform.SetSiblingIndex(rect.GetSiblingIndex());
            plate = (RectTransform)go.transform;
            plate.anchorMin = plate.anchorMax = plate.pivot = new Vector2(0.5f, 0.5f);

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            go.SetActive(false);
        }

        void OnDisable()
        {
            if (plate != null) plate.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (plate != null) Destroy(plate.gameObject);
        }

        void LateUpdate()
        {
            bool visible = !string.IsNullOrEmpty(text.text);
            if (plate.gameObject.activeSelf != visible) plate.gameObject.SetActive(visible);
            if (!visible) return;

            plate.position = rect.TransformPoint(rect.rect.center);
            plate.rotation = rect.rotation;
            plate.localScale = rect.localScale;
            plate.sizeDelta = new Vector2(text.preferredWidth, text.preferredHeight) + padding * 2f;
        }
    }
}
