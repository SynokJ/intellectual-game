using System.Collections.Generic;
using System.IO;
using IntellectualGame.Bazaar;
using IntellectualGame.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>
    /// Builds the Bazaar game (stall, dastarkhan, guests, goods) from primitive shapes into the open scene.
    /// <see cref="MenuSceneBuilder"/> calls <see cref="AddToScene"/> when the prefab is missing and then packs the objects
    /// into MiniGames/Bazaar/Prefabs/BazaarGame.prefab. Tools/Bazaar/Rebuild Game throws the prefab away and builds it again.
    /// </summary>
    internal static class BazaarSceneBuilder
    {
        internal const string PrefabPath = "Assets/_Project/MiniGames/Bazaar/Prefabs/BazaarGame.prefab";
        const string SpritesFolder = "Assets/_Project/MiniGames/Bazaar/Art";
        const string LevelPath = "Assets/_Project/MiniGames/Bazaar/Data/BazaarLevel01.asset";

        static readonly Color Wood = new Color(0.6f, 0.4f, 0.22f);
        static readonly Color DarkWood = new Color(0.42f, 0.26f, 0.13f);
        static readonly Color Red = new Color(0.78f, 0.2f, 0.2f);
        static readonly Color Gold = new Color(0.95f, 0.77f, 0.2f);
        static readonly Color Skin = new Color(0.95f, 0.8f, 0.65f);

        [MenuItem("Tools/Bazaar/Rebuild Game")]
        static void Rebuild()
        {
            if (File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
            MenuSceneBuilder.Build();
        }

        internal static void AddToScene(Canvas mainCanvas)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LevelPath));
            Directory.CreateDirectory(SpritesFolder);

            var sprites = CreateSprites(SpritesFolder);
            CreateLevel();

            foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == "BazaarGame") Object.DestroyImmediate(root);
            var oldHud = mainCanvas.transform.Find("Bazaar HUD");
            if (oldHud != null) Object.DestroyImmediate(oldHud.gameObject);

            var font = AppFont();

            // HUD (the menu builder moves it into the game's own canvas)
            var hud = new GameObject("Bazaar HUD", typeof(RectTransform));
            hud.transform.SetParent(mainCanvas.transform, false);
            Stretch((RectTransform)hud.transform);
            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safe.transform.SetParent(hud.transform, false);
            Stretch((RectTransform)safe.transform);

            var question = BandText(safe.transform, "Question Text", font, 54, FontStyle.Bold, 130f, 150f);
            var feedback = BandText(safe.transform, "Feedback Text", font, 44, FontStyle.Bold, 290f, 70f);
            var progress = MakeText(safe.transform, "Progress Text", "", 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(1f, 1f), new Vector2(-190f, -60f), new Vector2(340f, 60f));
            hud.SetActive(false);

            // world
            var world = new GameObject("BazaarGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var stage = stageObject.transform;

            BuildStall(stage, sprites);
            BuildDastarkhan(stage, sprites);
            var goods = Anchor(stage, "Goods Anchor", -3.5f, -1.05f);
            var price = Anchor(stage, "Price Anchor", -3.5f, 0.2f);
            var wallet = Anchor(stage, "Wallet Anchor", 4.6f, -2.25f);
            var guestSpot = Anchor(stage, "Guest Anchor", 4.6f, -1.95f);
            var jar = Anchor(stage, "Jar Anchor", -0.75f, -0.9f);

            var itemTemplate = BuildItemTemplate(stage, sprites);
            var guestTemplate = BuildGuestTemplate(stage, sprites, "Guest Template");
            var merchant = BuildGuestTemplate(stage, sprites, "Merchant");
            merchant.transform.position = new Vector3(-3.5f, -2.3f, 0f);
            merchant.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            merchant.Dress(new Color(0.2f, 0.45f, 0.35f), Color.white);
            merchant.gameObject.SetActive(true);
            var bubble = BuildBubbleTemplate(stage, sprites);
            var plate = BuildPlate(stage, sprites);

            var game = world.AddComponent<BazaarGame>();
            var stageView = world.AddComponent<BazaarStageView>();
            var hudView = world.AddComponent<BazaarHudView>();

            Assign(game, so =>
            {
                so.FindProperty("level").objectReferenceValue = AssetDatabase.LoadAssetAtPath<BazaarLevelData>(LevelPath);
                so.FindProperty("stage").objectReferenceValue = stageView;
                so.FindProperty("hud").objectReferenceValue = hudView;
            });
            Assign(stageView, so =>
            {
                so.FindProperty("stage").objectReferenceValue = stageObject;
                so.FindProperty("itemTemplate").objectReferenceValue = itemTemplate;
                so.FindProperty("guestTemplate").objectReferenceValue = guestTemplate;
                so.FindProperty("merchant").objectReferenceValue = merchant;
                so.FindProperty("bubbleTemplate").objectReferenceValue = bubble;
                so.FindProperty("plate").objectReferenceValue = plate;
                so.FindProperty("discSprite").objectReferenceValue = sprites["Circle"];
                so.FindProperty("blockSprite").objectReferenceValue = sprites["Square"];
                so.FindProperty("goodsAnchor").objectReferenceValue = goods;
                so.FindProperty("priceAnchor").objectReferenceValue = price;
                so.FindProperty("walletAnchor").objectReferenceValue = wallet;
                so.FindProperty("guestAnchor").objectReferenceValue = guestSpot;
                so.FindProperty("jarAnchor").objectReferenceValue = jar;
            });
            Assign(hudView, so =>
            {
                so.FindProperty("root").objectReferenceValue = hud;
                so.FindProperty("questionText").objectReferenceValue = question;
                so.FindProperty("feedbackText").objectReferenceValue = feedback;
                so.FindProperty("progressText").objectReferenceValue = progress;
            });

            stageObject.SetActive(false);
        }

        // ---------- world (primitive shapes) ----------

        static void BuildStall(Transform parent, Dictionary<string, Sprite> s)
        {
            var stall = Group("Stall", parent, Vector2.zero);

            Shape(stall, "Back Wall", s["Square"], new Vector2(-3.5f, -0.1f), new Vector2(6.4f, 4.2f), new Color(0.88f, 0.78f, 0.58f), 2);
            foreach (float x in new[] { -6.7f, -0.3f })
                Shape(stall, "Pole", s["Square"], new Vector2(x, -0.05f), new Vector2(0.16f, 5.1f), DarkWood, 4);

            Shape(stall, "Counter", s["Square"], new Vector2(-3.5f, -2.1f), new Vector2(6.4f, 1f), Wood, 6);
            Shape(stall, "Counter Trim", s["Square"], new Vector2(-3.5f, -1.66f), new Vector2(6.4f, 0.12f), DarkWood, 7);

            // striped awning with scalloped edge
            for (int i = 0; i < 8; i++)
            {
                float x = -3.5f + (i - 3.5f) * 0.9f;
                var color = i % 2 == 0 ? Red : Color.white;
                Shape(stall, "Awning " + i, s["Square"], new Vector2(x, 2.0f), new Vector2(0.9f, 0.6f), color, 8);
                Shape(stall, "Scallop " + i, s["Circle"], new Vector2(x, 1.7f), new Vector2(0.9f, 0.9f), color, 7);
            }

            // coin jar on the counter
            Shape(stall, "Jar", s["Square"], new Vector2(-0.75f, -1.2f), new Vector2(0.6f, 0.8f), new Color(0.55f, 0.35f, 0.2f), 8);
            Shape(stall, "Jar Coins", s["Circle"], new Vector2(-0.75f, -0.78f), new Vector2(0.5f, 0.5f), Gold, 9);
        }

        static void BuildDastarkhan(Transform parent, Dictionary<string, Sprite> s)
        {
            var cloth = Group("Dastarkhan", parent, Vector2.zero);
            Shape(cloth, "Cloth", s["Square"], new Vector2(4.6f, -2.25f), new Vector2(7.2f, 0.8f), Red, 12);
            Shape(cloth, "Cloth Edge", s["Square"], new Vector2(4.6f, -1.88f), new Vector2(7.2f, 0.1f), new Color(1f, 0.94f, 0.78f), 13);
            for (int i = -5; i <= 5; i++)
                Shape(cloth, "Pattern " + i, s["Square"], new Vector2(4.6f + i * 0.6f, -2.35f), new Vector2(0.18f, 0.18f), new Color(1f, 0.94f, 0.78f), 13, 45f);
        }

        static ItemView BuildItemTemplate(Transform parent, Dictionary<string, Sprite> s)
        {
            var root = Group("Item Template", parent, Vector2.zero);
            var body = ShapeRenderer(root, "Body", s["Square"], Vector2.zero, Vector2.one, Color.white, 0);
            var highlight = ShapeRenderer(root, "Highlight", s["Square"], new Vector2(-0.12f, 0.12f), new Vector2(0.5f, 0.5f), Color.white, 1);

            var item = root.gameObject.AddComponent<ItemView>();
            Assign(item, so =>
            {
                so.FindProperty("body").objectReferenceValue = body;
                so.FindProperty("highlight").objectReferenceValue = highlight;
            });
            root.gameObject.SetActive(false);
            return item;
        }

        static GuestView BuildGuestTemplate(Transform parent, Dictionary<string, Sprite> s, string name)
        {
            var root = Group(name, parent, Vector2.zero);
            var dark = new Color(0.15f, 0.12f, 0.1f);

            var robe = ShapeRenderer(root, "Robe", s["HalfCircle"], Vector2.zero, new Vector2(1f, 2f), Color.white, 0);
            ShapeRenderer(root, "Head", s["Circle"], new Vector2(0f, 1.15f), new Vector2(0.55f, 0.55f), Skin, 1);
            var hat = ShapeRenderer(root, "Hat", s["HalfCircle"], new Vector2(0f, 1.3f), new Vector2(0.6f, 0.7f), Color.white, 2);
            ShapeRenderer(root, "Hat Band", s["Square"], new Vector2(0f, 1.3f), new Vector2(0.6f, 0.08f), dark, 3);
            ShapeRenderer(root, "Eye Left", s["Circle"], new Vector2(-0.12f, 1.12f), new Vector2(0.07f, 0.07f), dark, 3);
            ShapeRenderer(root, "Eye Right", s["Circle"], new Vector2(0.12f, 1.12f), new Vector2(0.07f, 0.07f), dark, 3);

            var guest = root.gameObject.AddComponent<GuestView>();
            Assign(guest, so =>
            {
                so.FindProperty("robe").objectReferenceValue = robe;
                so.FindProperty("hat").objectReferenceValue = hat;
            });
            root.gameObject.SetActive(false);
            return guest;
        }

        static Transform BuildBubbleTemplate(Transform parent, Dictionary<string, Sprite> s)
        {
            var root = Group("Bubble Template", parent, Vector2.zero);
            var renderer = root.gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = s["Circle"];
            renderer.color = new Color(1f, 1f, 1f, 0.92f);
            root.localScale = new Vector3(1.5f, 1.4f, 1f);
            root.gameObject.SetActive(false);
            return root;
        }

        static Transform BuildPlate(Transform parent, Dictionary<string, Sprite> s)
        {
            var plate = Group("Beshbarmak Plate", parent, new Vector2(4.6f, -1.75f));
            Shape(plate, "Dish", s["Circle"], new Vector2(4.6f, -1.75f), new Vector2(2.8f, 0.7f), new Color(0.92f, 0.9f, 0.85f), 14);
            Shape(plate, "Noodles", s["HalfCircle"], new Vector2(4.6f, -1.65f), new Vector2(2.1f, 1.3f), new Color(0.98f, 0.94f, 0.8f), 15);
            for (int i = -2; i <= 2; i++)
                Shape(plate, "Meat " + i, s["Square"], new Vector2(4.6f + i * 0.36f, -1.1f + (i % 2 == 0 ? 0.08f : -0.04f)),
                    new Vector2(0.34f, 0.26f), new Color(0.65f, 0.25f, 0.2f), 16, i * 10f);
            for (int i = -1; i <= 1; i++)
                Shape(plate, "Onion " + i, s["Circle"], new Vector2(4.6f + i * 0.5f, -0.9f), new Vector2(0.24f, 0.24f), new Color(0.75f, 0.5f, 0.65f), 17);
            plate.gameObject.SetActive(false);
            return plate;
        }

        // ---------- level data ----------

        static void CreateLevel()
        {
            if (AssetDatabase.LoadAssetAtPath<BazaarLevelData>(LevelPath) != null) return;

            var level = ScriptableObject.CreateInstance<BazaarLevelData>();
            AssetDatabase.CreateAsset(level, LevelPath);

            var products = new (string one, string few, string many, Color color, ProductShape shape)[]
            {
                ("лепёшка", "лепёшки", "лепёшек", new Color(0.87f, 0.66f, 0.36f), ProductShape.Disc),
                ("луковица", "луковицы", "луковиц", new Color(0.75f, 0.45f, 0.62f), ProductShape.Disc),
                ("кусок мяса", "куска мяса", "кусков мяса", new Color(0.72f, 0.25f, 0.22f), ProductShape.Block),
                ("пачка лапши", "пачки лапши", "пачек лапши", new Color(0.95f, 0.87f, 0.6f), ProductShape.Block),
            };

            var so = new SerializedObject(level);
            var list = so.FindProperty("products");
            list.arraySize = products.Length;
            for (int i = 0; i < products.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("one").stringValue = products[i].one;
                item.FindPropertyRelative("few").stringValue = products[i].few;
                item.FindPropertyRelative("many").stringValue = products[i].many;
                item.FindPropertyRelative("color").colorValue = products[i].color;
                item.FindPropertyRelative("shape").enumValueIndex = (int)products[i].shape;
            }

            // fixed fallback rounds, used only when "randomize" is switched off
            var fallback = new[]
            {
                (BazaarRoundKind.Feed, 0, 3, 2),
                (BazaarRoundKind.Pay, 2, 2, 3),
                (BazaarRoundKind.Change, 1, 8, 5),
            };
            var rounds = so.FindProperty("rounds");
            rounds.arraySize = fallback.Length;
            for (int i = 0; i < fallback.Length; i++)
            {
                var item = rounds.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("kind").enumValueIndex = (int)fallback[i].Item1;
                item.FindPropertyRelative("productIndex").intValue = fallback[i].Item2;
                item.FindPropertyRelative("a").intValue = fallback[i].Item3;
                item.FindPropertyRelative("b").intValue = fallback[i].Item4;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }
    }
}
