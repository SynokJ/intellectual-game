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
    }
}
