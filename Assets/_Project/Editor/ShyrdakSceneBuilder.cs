using IntellectualGame.Core;
using IntellectualGame.Shyrdak;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>Builds the Shyrdak game: a sample carpet, a carpet to paint (8 parts each) and a numbered palette in the HUD.</summary>
    internal static class ShyrdakSceneBuilder
    {
        internal const string PrefabPath = "Assets/_Project/MiniGames/Shyrdak/Prefabs/ShyrdakGame.prefab";
        const string LevelPath = "Assets/_Project/MiniGames/Shyrdak/Data/ShyrdakLevel01.asset";

        [MenuItem("Tools/Shyrdak/Rebuild Game")]
        static void Rebuild()
        {
            if (System.IO.File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
            MenuSceneBuilder.Build();
        }

        internal static void AddToScene(Canvas mainCanvas)
        {
            System.IO.Directory.CreateDirectory(SharedSpritesFolder);
            var s = CreateSprites(SharedSpritesFolder);
            var level = EnsureAsset<ShyrdakLevelData>(LevelPath);

            RemoveOld("ShyrdakGame", "Shyrdak HUD", mainCanvas);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var hud = StandardHud(mainCanvas, "Shyrdak HUD", font, false);

            // palette row under the question
            var palette = new GameObject("Palette", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            palette.transform.SetParent(hud.safe, false);
            var paletteRect = (RectTransform)palette.transform;
            paletteRect.anchorMin = new Vector2(0f, 1f);
            paletteRect.anchorMax = new Vector2(1f, 1f);
            paletteRect.pivot = new Vector2(0.5f, 1f);
            paletteRect.offsetMin = new Vector2(0f, -490f);
            paletteRect.offsetMax = new Vector2(0f, -370f);
            var layout = palette.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 18f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var swatch = BuildSwatchTemplate(palette.transform, font);

            // world
            var world = new GameObject("ShyrdakGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var sample = BuildCarpet(stageObject.transform, s, "Sample Carpet", -4.6f, out var sampleParts);
            var paint = BuildCarpet(stageObject.transform, s, "Paint Carpet", 4.6f, out var paintParts);

            var game = world.AddComponent<ShyrdakGame>();
            var view = world.AddComponent<ShyrdakView>();
            var hudView = world.AddComponent<GameHudView>();
            Assign(game, so =>
            {
                so.FindProperty("level").objectReferenceValue = level;
                so.FindProperty("view").objectReferenceValue = view;
                so.FindProperty("hud").objectReferenceValue = hudView;
            });
            Assign(view, so =>
            {
                so.FindProperty("stage").objectReferenceValue = stageObject;
                so.FindProperty("paletteRoot").objectReferenceValue = palette;
                so.FindProperty("paletteContainer").objectReferenceValue = paletteRect;
                so.FindProperty("swatchTemplate").objectReferenceValue = swatch;
                SetArray(so.FindProperty("sampleParts"), sampleParts);
                SetArray(so.FindProperty("paintParts"), paintParts);
                so.FindProperty("paintCarpet").objectReferenceValue = paint;
            });
            WireHud(hudView, hud);
            stageObject.SetActive(false);
        }

        /// <summary>A carpet of 8 single-shape parts; the order is the order the player paints them.</summary>
        static Transform BuildCarpet(Transform parent, System.Collections.Generic.Dictionary<string, Sprite> s, string name, float x,
            out SpriteRenderer[] parts)
        {
            const float y = -0.9f;
            var carpet = Group(name, parent, new Vector2(x, y));
            Shape(carpet, "Fringe", s["Square"], new Vector2(x, y), new Vector2(5.1f, 3.3f), new Color(0.3f, 0.2f, 0.12f), 0);

            var list = new System.Collections.Generic.List<SpriteRenderer>
            {
                ShapeRenderer(carpet, "Part 1 Field", s["Square"], new Vector2(x, y), new Vector2(4.8f, 3f), Color.gray, 1),
                ShapeRenderer(carpet, "Part 2 Stripe", s["Square"], new Vector2(x, y - 1.25f), new Vector2(4.8f, 0.4f), Color.gray, 2),
                ShapeRenderer(carpet, "Part 3 Diamond", s["Square"], new Vector2(x, y), new Vector2(2f, 2f), Color.gray, 3, 45f),
                ShapeRenderer(carpet, "Part 4 Circle", s["Circle"], new Vector2(x, y), new Vector2(1f, 1f), Color.gray, 5),
            };
            int corner = 5;
            foreach (var offset in new[] { new Vector2(-1.9f, 0.95f), new Vector2(1.9f, 0.95f), new Vector2(-1.9f, -0.55f), new Vector2(1.9f, -0.55f) })
                list.Add(ShapeRenderer(carpet, "Part " + corner++ + " Corner", s["Square"], new Vector2(x, y) + offset, new Vector2(0.7f, 0.7f), Color.gray, 4, 45f));

            parts = list.ToArray();
            return carpet;
        }

        static GameObject BuildSwatchTemplate(Transform parent, Font font)
        {
            var go = new GameObject("Swatch Template", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(100f, 100f);
            var number = MakeText(go.transform, "Number", "", 52, Ink, TextAnchor.MiddleCenter, font, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Stretch(number.rectTransform);
            var outline = number.gameObject.AddComponent<Outline>();
            outline.effectColor = Color.white;
            outline.effectDistance = new Vector2(2f, -2f);
            go.SetActive(false);
            return go;
        }
    }
}
