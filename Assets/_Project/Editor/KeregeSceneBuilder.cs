using IntellectualGame.Core;
using IntellectualGame.Kerege;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>Builds the Kerege-words game: a lattice wall behind, word slots and numbered letter tiles in the HUD.</summary>
    internal static class KeregeSceneBuilder
    {
        internal const string PrefabPath = "Assets/_Project/MiniGames/Kerege/Prefabs/KeregeGame.prefab";
        const string LevelPath = "Assets/_Project/MiniGames/Kerege/Data/KeregeLevel01.asset";

        static readonly (string word, string hint)[] Words =
        {
            ("тоо", "гора"), ("кой", "овца"), ("жүн", "шерсть"), ("көл", "озеро"), ("бөрү", "волк"), ("алма", "яблоко"),
            ("нан", "хлеб"), ("эне", "мать"), ("ата", "отец"), ("күн", "солнце"), ("чай", "чай"), ("суу", "вода"),
        };

        [MenuItem("Tools/Kerege/Rebuild Game")]
        static void Rebuild()
        {
            if (System.IO.File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
            MenuSceneBuilder.Build();
        }

        internal static void AddToScene(Canvas mainCanvas)
        {
            System.IO.Directory.CreateDirectory(SharedSpritesFolder);
            var s = CreateSprites(SharedSpritesFolder);
            var level = EnsureAsset<KeregeLevelData>(LevelPath, so =>
            {
                var list = so.FindProperty("words");
                list.arraySize = Words.Length;
                for (int i = 0; i < Words.Length; i++)
                {
                    var item = list.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("word").stringValue = Words[i].word;
                    item.FindPropertyRelative("hint").stringValue = Words[i].hint;
                }
            });

            RemoveOld("KeregeGame", "Kerege HUD", mainCanvas);
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var hud = StandardHud(mainCanvas, "Kerege HUD", font, false);

            // letters: two rows of tiles, anchored by screen fraction so they adapt to any aspect
            var letters = new GameObject("Letters", typeof(RectTransform));
            letters.transform.SetParent(hud.safe, false);
            Stretch((RectTransform)letters.transform);
            var slotRow = TileRow(letters.transform, "Word Slots", 0.58f);
            var tileRow = TileRow(letters.transform, "Letter Tiles", 0.36f);
            var tileTemplate = BuildTileTemplate(tileRow, font, uiSprite);

            // world: a kerege wall - wooden frame with a diamond lattice
            var world = new GameObject("KeregeGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var lattice = Group("Lattice", stageObject.transform, Vector2.zero);
            Shape(lattice, "Wall", s["Square"], new Vector2(0f, -0.2f), new Vector2(12f, 4.4f), new Color(0.72f, 0.52f, 0.3f), 2);
            for (int row = 0; row < 4; row++)
                for (int col = 0; col < 14; col++)
                    Shape(lattice, $"Diamond {row}.{col}", s["Square"], new Vector2(-6f + col * 0.9f + (row % 2) * 0.45f, -1.9f + row * 1.15f),
                        new Vector2(0.55f, 0.55f), new Color(0.5f, 0.32f, 0.16f), 3, 45f);

            var game = world.AddComponent<KeregeGame>();
            var view = world.AddComponent<KeregeView>();
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
                so.FindProperty("lettersRoot").objectReferenceValue = letters;
                so.FindProperty("slotContainer").objectReferenceValue = slotRow;
                so.FindProperty("tileContainer").objectReferenceValue = tileRow;
                so.FindProperty("tileTemplate").objectReferenceValue = tileTemplate;
                so.FindProperty("lattice").objectReferenceValue = lattice;
            });
            WireHud(hudView, hud);
            stageObject.SetActive(false);
        }

        static RectTransform TileRow(Transform parent, string name, float anchorY)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = new Vector2(0f, anchorY);
            rect.anchorMax = new Vector2(1f, anchorY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(0f, 170f);

            var layout = go.GetComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 20f;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            return rect;
        }

        static LetterTileView BuildTileTemplate(Transform parent, Font font, Sprite uiSprite)
        {
            var go = new GameObject("Tile Template", typeof(RectTransform), typeof(Image), typeof(CanvasGroup), typeof(LetterTileView));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(150f, 150f);
            var image = go.GetComponent<Image>();
            image.sprite = uiSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(1f, 0.94f, 0.78f);

            var letter = MakeText(go.transform, "Letter", "", 84, Ink, TextAnchor.MiddleCenter, font, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Stretch(letter.rectTransform);
            var number = MakeText(go.transform, "Number", "", 34, new Color(0.5f, 0.2f, 0.15f), TextAnchor.LowerRight, font, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Stretch(number.rectTransform);
            number.rectTransform.offsetMax = new Vector2(-12f, 0f);
            number.rectTransform.offsetMin = new Vector2(0f, 8f);

            var view = go.GetComponent<LetterTileView>();
            Assign(view, so =>
            {
                so.FindProperty("letterText").objectReferenceValue = letter;
                so.FindProperty("numberText").objectReferenceValue = number;
                so.FindProperty("group").objectReferenceValue = go.GetComponent<CanvasGroup>();
            });
            go.SetActive(false);
            return view;
        }
    }
}
