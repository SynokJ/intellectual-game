using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.EditorTools
{
    /// <summary>Helpers shared by the per-game scene builders: primitive shapes, sprites, UI widgets, serialization.</summary>
    internal static class BuilderUtils
    {
        const string FontPath = "Assets/_Project/Shared/Fonts/Inter-Regular.ttf";

        /// <summary>
        /// The font of all UI text. Unity's built-in font has no Cyrillic glyphs in a WebGL build (there are no OS fonts to
        /// fall back to), so the project embeds Inter, which covers Russian and the Kyrgyz letters ң, ө, ү.
        /// </summary>
        internal static Font AppFont() => AssetDatabase.LoadAssetAtPath<Font>(FontPath);

        internal static readonly Color Ink = new Color(0.12f, 0.16f, 0.27f);

        internal static Transform Group(string name, Transform parent, Vector2 center)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = center;
            return go.transform;
        }

        internal static void Shape(Transform parent, string name, Sprite sprite, Vector2 worldPos, Vector2 size, Color color,
            int order, float rotation = 0f)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;
        }

        // ---------- sprites ----------

        internal static Dictionary<string, Sprite> CreateSprites(string folder)
        {
            const int size = 128;
            var result = new Dictionary<string, Sprite>
            {
                ["Square"] = MakeSprite(folder, "Square", size, size, (x, y) => 1f, SpriteAlignment.Center),
                ["Circle"] = MakeSprite(folder, "Circle", size, size, (x, y) =>
                    Mathf.Clamp01(size / 2f - Vector2.Distance(new Vector2(x, y), new Vector2(size / 2f, size / 2f)) + 0.5f),
                    SpriteAlignment.Center),
                ["HalfCircle"] = MakeSprite(folder, "HalfCircle", size, size / 2, (x, y) =>
                    Mathf.Clamp01(size / 2f - Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(size / 2f, 0f)) + 0.5f),
                    SpriteAlignment.BottomCenter),
                ["Triangle"] = MakeSprite(folder, "Triangle", size, size, (x, y) =>
                    Mathf.Clamp01((size / 2f) * (1f - (y + 0.5f) / size) - Mathf.Abs(x + 0.5f - size / 2f) + 0.5f),
                    SpriteAlignment.BottomCenter),
            };
            return result;
        }

        internal static Sprite MakeSprite(string folder, string name, int width, int height, Func<int, int, float> alpha, SpriteAlignment alignment)
        {
            string path = $"{folder}/{name}.png";
            if (!File.Exists(path))
            {
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                var pixels = new Color[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        pixels[y * width + x] = new Color(1f, 1f, 1f, alpha(x, y));
                texture.SetPixels(pixels);
                File.WriteAllBytes(path, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            }

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = width;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)alignment;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // ---------- UI helpers ----------

        /// <summary>The in-game HUD canvas. Looked up by name because the scene also has a separate "Menu Canvas".</summary>
        internal static Canvas FindMainCanvas()
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if (canvas.name == "Canvas") return canvas;
            return null;
        }

        internal static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        internal static Text MakeText(Transform parent, string name, string value, int fontSize, Color color, TextAnchor alignment,
            Font font, Vector2 anchor, Vector2 position, Vector2 size, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var text = go.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        internal static Button MakeButton(Transform parent, string name, string label, Font font, Sprite sprite,
            Vector2 anchor, Vector2 position, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            go.GetComponent<Button>().targetGraphic = image;

            var text = MakeText(go.transform, "Text", label, 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, size);
            Stretch(text.rectTransform);
            return go.GetComponent<Button>();
        }

        // ---------- serialization helpers ----------

        internal static void Assign(UnityEngine.Object target, Action<SerializedObject> fill)
        {
            var so = new SerializedObject(target);
            fill(so);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static void SetArray<T>(SerializedProperty property, IList<T> items) where T : UnityEngine.Object
        {
            property.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        // ---------- shared helpers of the game builders ----------

        internal const string SharedSpritesFolder = "Assets/_Project/Shared/Art";

        internal struct HudParts
        {
            public GameObject root;
            public Transform safe;
            public Text question, options, feedback, progress;
        }

        /// <summary>The standard game HUD: question, options, feedback and round counter inside a safe area. Starts hidden.</summary>
        internal static HudParts StandardHud(Canvas canvas, string hudName, Font font, bool quiz)
        {
            var hud = new GameObject(hudName, typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)hud.transform);
            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(IntellectualGame.Core.SafeAreaFitter));
            safe.transform.SetParent(hud.transform, false);
            Stretch((RectTransform)safe.transform);

            var parts = new HudParts { root = hud, safe = safe.transform };
            parts.question = BandText(safe.transform, "Question Text", font, 54, FontStyle.Bold, 130f, 150f);
            parts.options = BandText(safe.transform, "Options Text", font, 44, FontStyle.Bold, 290f, 260f);
            parts.feedback = BandText(safe.transform, "Feedback Text", font, 44, FontStyle.Bold, quiz ? 560f : 290f, 60f);
            parts.progress = MakeText(safe.transform, "Progress Text", "", 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(1f, 1f), new Vector2(-190f, -60f), new Vector2(340f, 60f));
            hud.SetActive(false);
            return parts;
        }

        internal static void WireHud(IntellectualGame.Core.GameHudView view, HudParts hud) => Assign(view, so =>
        {
            so.FindProperty("root").objectReferenceValue = hud.root;
            so.FindProperty("questionText").objectReferenceValue = hud.question;
            so.FindProperty("optionsText").objectReferenceValue = hud.options;
            so.FindProperty("feedbackText").objectReferenceValue = hud.feedback;
            so.FindProperty("progressText").objectReferenceValue = hud.progress;
        });

        /// <summary>Removes the leftovers of an earlier build of a game from the open scene.</summary>
        internal static void RemoveOld(string rootName, string hudName, Canvas canvas)
        {
            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == rootName) UnityEngine.Object.DestroyImmediate(root);
            var hud = canvas.transform.Find(hudName);
            if (hud != null) UnityEngine.Object.DestroyImmediate(hud.gameObject);
        }

        /// <summary>Loads the asset at the path, or creates it and lets <paramref name="fill"/> set its data.</summary>
        internal static T EnsureAsset<T>(string path, Action<SerializedObject> fill = null) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            if (fill != null)
            {
                var so = new SerializedObject(asset);
                fill(so);
                so.ApplyModifiedPropertiesWithoutUndo();
            }
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        internal static Transform Anchor(Transform parent, string name, float x, float y) => Group(name, parent, new Vector2(x, y));

        internal static SpriteRenderer ShapeRenderer(Transform parent, string name, Sprite sprite, Vector2 position, Vector2 size, Color color, int order,
            float rotation = 0f)
        {
            Shape(parent, name, sprite, position, size, color, order, rotation);
            return parent.Find(name).GetComponent<SpriteRenderer>();
        }

        /// <summary>A text that stretches across the screen between margins, so it adapts to landscape and portrait.</summary>
        internal static Text BandText(Transform parent, string name, Font font, int maxSize, FontStyle style, float top, float height)
        {
            const float sideMargin = 260f;
            var text = MakeText(parent, name, "", maxSize, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, style);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(sideMargin, -(top + height));
            rect.offsetMax = new Vector2(-sideMargin, -top);

            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 24;
            text.resizeTextMaxSize = maxSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

    }
}
