using System;
using System.IO;
using IntellectualGame.Core;
using IntellectualGame.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>
    /// Builds the adaptive UI in SampleScene: catalog asset, "Menu Canvas" with game cards, the runtime answer pad,
    /// readable HUD text plates and the wiring into the GameController.
    /// Runs once automatically after the first compile and can be re-run from Tools/Menu/Build Menu.
    /// </summary>
    public static class MenuSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string CatalogPath = "Assets/_Project/Menu/Data/GameCatalog.asset";
        const string CirclePath = "Assets/_Project/MiniGames/Yurta/Art/Circle.png";
        const string MarkerPath = "Library/MenuSceneBuilt_v2.txt";

        static readonly Color Sky = new Color(0.62f, 0.82f, 0.95f);
        static readonly Color Grass = new Color(0.42f, 0.67f, 0.31f);
        static readonly Color Plate = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color Cream = new Color(1f, 0.94f, 0.78f);

        static readonly string[] HudTexts = { "Hint Text", "Equation Text", "Progress Text", "Feedback Text", "Question Text", "Event Text" };
        static readonly string[] HudButtons = { "Quit Button", "Replay Button", "Menu Button" };

        [MenuItem("Tools/Menu/Build Menu")]
        static void BuildFromMenu() => Build();

        [InitializeOnLoadMethod]
        static void AutoBuild()
        {
            EditorApplication.delayCall += () =>
            {
                if (File.Exists(MarkerPath) || EditorApplication.isPlayingOrWillChangePlaymode) return;
                if (EditorSceneManager.GetActiveScene().isDirty) return;
                Build();
            };
        }

        static void Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(CatalogPath));
            var catalog = CreateCatalog();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<GameController>();
            var mainCanvas = FindMainCanvas();
            if (controller == null || mainCanvas == null)
            {
                Debug.LogError("Menu: build the game scenes first (Tools/Yurta/Build Scene, Tools/Otara/Build Scene) - GameController is missing.");
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "Menu Canvas") UnityEngine.Object.DestroyImmediate(root);
            foreach (var old in new[] { "Action Button", "Answer Pad" })
            {
                var oldObject = mainCanvas.transform.Find(old);
                if (oldObject != null) UnityEngine.Object.DestroyImmediate(oldObject.gameObject);
            }

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            var menu = BuildMenu(font, uiSprite, circle);
            var pad = BuildAnswerPad(mainCanvas, font, uiSprite);
            StyleHud(mainCanvas, uiSprite);

            var games = UnityEngine.Object.FindObjectsByType<Game>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assign(controller, so =>
            {
                so.FindProperty("catalog").objectReferenceValue = catalog;
                so.FindProperty("menuView").objectReferenceValue = menu;
                so.FindProperty("answerPad").objectReferenceValue = pad;
                SetArray(so.FindProperty("games"), games);
            });

            var win = mainCanvas.transform.Find("Win Panel");
            if (win != null) win.SetAsLastSibling();

            var camera = Camera.main;
            if (camera != null && camera.GetComponent<CameraFit>() == null) camera.gameObject.AddComponent<CameraFit>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, DateTime.Now.ToString("o"));
            Debug.Log("Adaptive UI built (menu, answer pad, text plates). Press Play.");
        }

        // ---------- main menu ----------

        static MainMenuView BuildMenu(Font font, Sprite uiSprite, Sprite circle)
        {
            // own canvas and scaler so the menu can switch between landscape and portrait references
            var canvasObject = new GameObject("Menu Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster),
                typeof(AdaptiveCanvasScaler));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            var view = canvasObject.AddComponent<MainMenuView>();

            // opaque background with a simple landscape, content stays inside the safe area
            var panel = new GameObject("Menu Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)panel.transform);
            panel.GetComponent<Image>().color = Sky;

            Decor(panel.transform, "Sun", circle, new Vector2(1f, 1f), new Vector2(-140f, -120f), new Vector2(240f, 240f),
                new Color(1f, 0.87f, 0.3f));
            Decor(panel.transform, "Hill Left", circle, new Vector2(0f, 0f), new Vector2(300f, -330f), new Vector2(1000f, 1000f),
                new Color(0.5f, 0.74f, 0.4f));
            Decor(panel.transform, "Hill Right", circle, new Vector2(1f, 0f), new Vector2(-250f, -380f), new Vector2(900f, 900f),
                new Color(0.46f, 0.7f, 0.36f));
            var ground = Decor(panel.transform, "Ground", null, new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 160f), Grass);
            ground.anchorMin = new Vector2(0f, 0f);
            ground.anchorMax = new Vector2(1f, 0f);
            ground.pivot = new Vector2(0.5f, 0f);

            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safe.transform.SetParent(panel.transform, false);
            Stretch((RectTransform)safe.transform);

            var title = MakeText(safe.transform, "Title Text", "", 80, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -80f), new Vector2(1000f, 110f), FontStyle.Bold);
            var subtitle = MakeText(safe.transform, "Subtitle Text", "", 44, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -200f), new Vector2(800f, 70f));
            AddBackdrop(title, uiSprite);
            AddBackdrop(subtitle, uiSprite);

            // scrollable, adaptive grid of cards
            var scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(safe.transform, false);
            var scrollRect = (RectTransform)scrollObject.transform;
            Stretch(scrollRect);
            scrollRect.offsetMin = new Vector2(40f, 40f);
            scrollRect.offsetMax = new Vector2(-40f, -290f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D), typeof(Image));
            viewport.transform.SetParent(scrollObject.transform, false);
            Stretch((RectTransform)viewport.transform);
            viewport.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);

            var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter),
                typeof(AdaptiveGrid));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = (RectTransform)content.transform;
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.spacing = new Vector2(40f, 40f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Assign(content.GetComponent<AdaptiveGrid>(), so =>
            {
                so.FindProperty("minCellWidth").floatValue = 520f;
                so.FindProperty("maxCellWidth").floatValue = 720f;
                so.FindProperty("cellAspect").floatValue = 0.72f;
                so.FindProperty("portraitCellAspect").floatValue = 0.6f;
                so.FindProperty("maxColumns").intValue = 4;
            });

            var scroll = scrollObject.GetComponent<ScrollRect>();
            scroll.content = contentRect;
            scroll.viewport = (RectTransform)viewport.transform;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;

            var card = BuildCardTemplate(content.transform, font, uiSprite, circle);

            Assign(view, so =>
            {
                so.FindProperty("root").objectReferenceValue = panel;
                so.FindProperty("titleText").objectReferenceValue = title;
                so.FindProperty("subtitleText").objectReferenceValue = subtitle;
                so.FindProperty("cardContainer").objectReferenceValue = contentRect;
                so.FindProperty("cardTemplate").objectReferenceValue = card;
            });
            return view;
        }

        static GameCardView BuildCardTemplate(Transform parent, Font font, Sprite uiSprite, Sprite circle)
        {
            var go = new GameObject("Game Card Template", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(CanvasGroup), typeof(GameCardView));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(560f, 400f);

            var background = go.GetComponent<Image>();
            background.sprite = uiSprite;
            background.type = Image.Type.Sliced;
            var button = go.GetComponent<Button>();
            button.targetGraphic = background;
            var colors = button.colors;
            colors.highlightedColor = new Color(0.93f, 0.93f, 0.93f);
            colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
            button.colors = colors;

            // soft light blob in the upper part of the card
            var blob = Decor(go.transform, "Blob", circle, new Vector2(1f, 1f), new Vector2(-40f, -20f), new Vector2(230f, 230f),
                new Color(1f, 1f, 1f, 0.28f));

            // number badge, top left
            var badge = Decor(go.transform, "Number Badge", circle, new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(84f, 84f), Plate);
            badge.pivot = new Vector2(0f, 1f);
            var number = MakeText(badge, "Number Text", "", 48, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, badge.sizeDelta, FontStyle.Bold);
            Stretch(number.rectTransform);

            // "coming soon" badge, top right
            var soon = Decor(go.transform, "Soon Badge", uiSprite, new Vector2(1f, 1f), new Vector2(-28f, -36f), new Vector2(170f, 60f), Plate);
            soon.pivot = new Vector2(1f, 1f);
            soon.GetComponent<Image>().type = Image.Type.Sliced;
            var soonText = MakeText(soon, "Soon Text", "Скоро", 32, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, soon.sizeDelta, FontStyle.Bold);
            Stretch(soonText.rectTransform);
            soon.gameObject.SetActive(false);

            // transparent black plate with the readable texts
            var plate = Decor(go.transform, "Text Plate", uiSprite, new Vector2(0.5f, 0f), Vector2.zero, Vector2.zero, Plate);
            plate.GetComponent<Image>().type = Image.Type.Sliced;
            plate.anchorMin = new Vector2(0.05f, 0.05f);
            plate.anchorMax = new Vector2(0.95f, 0.58f);
            plate.offsetMin = Vector2.zero;
            plate.offsetMax = Vector2.zero;

            var title = MakeText(plate, "Title Text", "", 56, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Fill(title.rectTransform, 0.5f, 1f);
            FitText(title, 28, 56);

            var description = MakeText(plate, "Description Text", "", 34, Color.white, TextAnchor.UpperCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Fill(description.rectTransform, 0f, 0.5f);
            FitText(description, 20, 34);

            var card = go.GetComponent<GameCardView>();
            Assign(card, so =>
            {
                so.FindProperty("background").objectReferenceValue = background;
                so.FindProperty("numberText").objectReferenceValue = number;
                so.FindProperty("titleText").objectReferenceValue = title;
                so.FindProperty("descriptionText").objectReferenceValue = description;
                so.FindProperty("soonBadge").objectReferenceValue = soon.gameObject;
                so.FindProperty("button").objectReferenceValue = button;
                so.FindProperty("group").objectReferenceValue = go.GetComponent<CanvasGroup>();
            });
            go.SetActive(false);
            return card;
        }

        // ---------- in-game: answer pad and readable HUD ----------

        static AnswerPadView BuildAnswerPad(Canvas mainCanvas, Font font, Sprite uiSprite)
        {
            var padObject = new GameObject("Answer Pad", typeof(RectTransform), typeof(AnswerPadView));
            padObject.transform.SetParent(mainCanvas.transform, false);
            Stretch((RectTransform)padObject.transform);

            var panel = new GameObject("Answer Panel", typeof(RectTransform), typeof(Image), typeof(GridLayoutGroup),
                typeof(ContentSizeFitter), typeof(AdaptiveGrid));
            panel.transform.SetParent(padObject.transform, false);
            var panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.4f);

            var grid = panel.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(24, 24, 24, 24);
            grid.spacing = new Vector2(20f, 20f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            panel.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            Assign(panel.GetComponent<AdaptiveGrid>(), so =>
            {
                so.FindProperty("minCellWidth").floatValue = 150f;
                so.FindProperty("maxCellWidth").floatValue = 200f;
                so.FindProperty("cellAspect").floatValue = 1f;
                so.FindProperty("portraitCellAspect").floatValue = 0f;
                so.FindProperty("maxColumns").intValue = 10;
            });

            var template = MakeButton(panel.transform, "Answer Button Template", "", font, uiSprite,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180f, 180f));
            template.GetComponent<Image>().color = Cream;
            var label = template.GetComponentInChildren<Text>();
            label.fontSize = 84;
            label.fontStyle = FontStyle.Bold;
            template.gameObject.SetActive(false);

            var pad = padObject.GetComponent<AnswerPadView>();
            Assign(pad, so =>
            {
                so.FindProperty("root").objectReferenceValue = panel;
                so.FindProperty("container").objectReferenceValue = panelRect;
                so.FindProperty("buttonTemplate").objectReferenceValue = template;
            });
            return pad;
        }

        static void StyleHud(Canvas mainCanvas, Sprite uiSprite)
        {
            foreach (var text in mainCanvas.GetComponentsInChildren<Text>(true))
            {
                if (Array.IndexOf(HudTexts, text.name) < 0) continue;
                text.color = Color.white;
                text.fontStyle = FontStyle.Bold;
                AddBackdrop(text, uiSprite);
            }

            foreach (var image in mainCanvas.GetComponentsInChildren<Image>(true))
                if (Array.IndexOf(HudButtons, image.name) >= 0) image.color = Cream;
        }

        // ---------- helpers ----------

        static void AddBackdrop(Text text, Sprite sprite)
        {
            var backdrop = text.GetComponent<TextBackdrop>();
            if (backdrop == null) backdrop = text.gameObject.AddComponent<TextBackdrop>();
            Assign(backdrop, so => so.FindProperty("sprite").objectReferenceValue = sprite);
        }

        static RectTransform Decor(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 position, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        static void Fill(RectTransform rect, float minY, float maxY)
        {
            rect.anchorMin = new Vector2(0f, minY);
            rect.anchorMax = new Vector2(1f, maxY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(24f, 8f);
            rect.offsetMax = new Vector2(-24f, -8f);
        }

        static void FitText(Text text, int minSize, int maxSize)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
        }

        static GameCatalog CreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog != null) return catalog;

            catalog = ScriptableObject.CreateInstance<GameCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);

            var entries = new[]
            {
                (1, "Юрта: сборка", "Реши примеры и собери юрту", new Color(0.99f, 0.8f, 0.4f)),
                (2, "Отара овец", "Считай овец в загоне и раздай их семьям", new Color(0.6f, 0.85f, 0.55f)),
            };
            var so = new SerializedObject(catalog);
            var list = so.FindProperty("entries");
            list.arraySize = entries.Length;
            for (int i = 0; i < entries.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("menuNumber").intValue = entries[i].Item1;
                item.FindPropertyRelative("title").stringValue = entries[i].Item2;
                item.FindPropertyRelative("description").stringValue = entries[i].Item3;
                item.FindPropertyRelative("accent").colorValue = entries[i].Item4;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
