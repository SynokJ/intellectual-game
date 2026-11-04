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
        const string MarkerPath = "Library/MenuSceneBuilt_v5.txt";

        struct GameSpec
        {
            public string rootName, hudName, prefabPath;
            public int menuNumber;
            public Color padColor;

            public GameSpec(string rootName, string hudName, string prefabPath, int menuNumber, Color padColor)
            {
                this.rootName = rootName;
                this.hudName = hudName;
                this.prefabPath = prefabPath;
                this.menuNumber = menuNumber;
                this.padColor = padColor;
            }
        }

        static readonly GameSpec[] Specs =
        {
            new GameSpec("YurtaGame", "Yurta HUD", "Assets/_Project/MiniGames/Yurta/Prefabs/YurtaGame.prefab", 1, new Color(1f, 0.9f, 0.62f)),
            new GameSpec("OtaraGame", "Otara HUD", "Assets/_Project/MiniGames/Otara/Prefabs/OtaraGame.prefab", 2, new Color(0.82f, 0.95f, 0.75f)),
            new GameSpec("BazaarGame", "Bazaar HUD", BazaarSceneBuilder.PrefabPath, 3, new Color(1f, 0.82f, 0.7f)),
        };

        static readonly Color Sky = new Color(0.62f, 0.82f, 0.95f);
        static readonly Color Grass = new Color(0.42f, 0.67f, 0.31f);
        static readonly Color Plate = new Color(0f, 0f, 0f, 0.6f);
        static readonly Color Cream = new Color(1f, 0.94f, 0.78f);

        // the categories and the first games of each; games without a prefab are shown as "coming soon"
        static readonly (string title, Color accent)[] Categories =
        {
            ("Математика", new Color(0.99f, 0.8f, 0.4f)),
            ("Физика и логика", new Color(0.55f, 0.75f, 0.95f)),
            ("Азбука и язык", new Color(0.95f, 0.65f, 0.7f)),
            ("Ребусы", new Color(0.75f, 0.65f, 0.95f)),
            ("Раскраски", new Color(0.95f, 0.75f, 0.5f)),
            ("Познавательные игры", new Color(0.55f, 0.85f, 0.8f)),
            ("Дополнительно", new Color(0.8f, 0.8f, 0.55f)),
        };

        static readonly (int number, int category, string title, string description)[] CatalogGames =
        {
            (1, 0, "Юрта: сборка", "Реши примеры и собери юрту"),
            (2, 0, "Отара овец", "Считай овец в загоне и раздай их семьям"),
            (3, 0, "Базар и бешбармак", "Накорми гостей: считай продукты, монеты и сдачу"),
            (4, 1, "Альчик", "Выбери силу и угол броска и попади в цель"),
            (5, 1, "Көк бөрү: передача", "Брось козла в тай-казан, учитывая инерцию"),
            (6, 1, "Лук и мишень", "Учти ветер и расстояние и попади в центр"),
            (7, 2, "Кереге-слова", "Собери слово из букв на решётке кереге"),
            (8, 2, "Алфавит-путешествие", "Найди пары: буква и предмет кыргызской культуры"),
            (9, 2, "Пиши по войлоку", "Обведи букву пальцем по войлочному шаблону"),
            (10, 3, "Ребус-пословица", "Разгадай ребус и открой народную мудрость"),
            (11, 3, "Загадки-жомок", "Выбери верную картинку к народной загадке"),
            (12, 3, "Предмет по тени", "Угадай предмет быта по силуэту"),
            (13, 4, "Шырдак", "Раскрась войлочный ковёр по образцу"),
            (14, 4, "Тебетей и элечек", "Раскрась и укрась национальный головной убор"),
            (15, 4, "Джайлоо-пейзаж", "Раскрась горы, озеро и юрты"),
            (16, 5, "Манас: путь героя", "Отвечай на вопросы по эпосу и веди героя вперёд"),
            (17, 5, "Карта Кыргызстана", "Расставь горы, озёра и города на карте"),
            (18, 5, "Животные и птицы", "Найди пару: животное и место, где оно живёт"),
            (19, 6, "Память: узоры", "Найди одинаковые узоры и предметы"),
            (20, 6, "Ремёсла: валяние войлока", "Расставь шаги валяния войлока по порядку"),
            (21, 6, "Путь каравана", "Проложи маршрут по Великому шёлковому пути"),
        };

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

        internal static void Build()
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

            if (!File.Exists(BazaarSceneBuilder.PrefabPath)) BazaarSceneBuilder.AddToScene(mainCanvas);

            // the shell canvas (hint, quit, win panel) draws above the game canvases, the menu above everything
            mainCanvas.sortingOrder = 5;
            var menu = BuildMenu(font, uiSprite, circle);
            StyleHud(mainCanvas.transform, uiSprite);

            var prefabs = new Game[Specs.Length];
            for (int i = 0; i < Specs.Length; i++) prefabs[i] = Pack(Specs[i], mainCanvas, scene, font, uiSprite);
            AssignPrefabs(catalog, prefabs);
            EnsureScenery(prefabs);

            Assign(controller, so =>
            {
                so.FindProperty("catalog").objectReferenceValue = catalog;
                so.FindProperty("menuView").objectReferenceValue = menu;
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
            // body: category tabs on top, the scrollable cards take the rest
            var body = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup));
            body.transform.SetParent(safe.transform, false);
            var bodyRect = (RectTransform)body.transform;
            Stretch(bodyRect);
            bodyRect.offsetMin = new Vector2(40f, 40f);
            bodyRect.offsetMax = new Vector2(-40f, -270f);
            var bodyLayout = body.GetComponent<VerticalLayoutGroup>();
            bodyLayout.spacing = 16f;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = false;

            var tabsObject = new GameObject("Category Tabs", typeof(RectTransform), typeof(GridLayoutGroup), typeof(AdaptiveGrid));
            tabsObject.transform.SetParent(body.transform, false);
            var tabGrid = tabsObject.GetComponent<GridLayoutGroup>();
            tabGrid.spacing = new Vector2(16f, 16f);
            tabGrid.childAlignment = TextAnchor.UpperCenter;
            tabGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            Assign(tabsObject.GetComponent<AdaptiveGrid>(), so =>
            {
                so.FindProperty("minCellWidth").floatValue = 260f;
                so.FindProperty("maxCellWidth").floatValue = 360f;
                so.FindProperty("cellAspect").floatValue = 0.3f;
                so.FindProperty("portraitCellAspect").floatValue = 0.34f;
                so.FindProperty("maxColumns").intValue = 7;
            });
            var tab = BuildTabTemplate(tabsObject.transform, font, uiSprite);

            var scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect), typeof(LayoutElement));
            scrollObject.transform.SetParent(body.transform, false);
            var scrollLayout = scrollObject.GetComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1f;
            scrollLayout.minHeight = 300f;

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
                so.FindProperty("tabContainer").objectReferenceValue = (RectTransform)tabsObject.transform;
                so.FindProperty("tabTemplate").objectReferenceValue = tab;
                so.FindProperty("cardContainer").objectReferenceValue = contentRect;
                so.FindProperty("cardTemplate").objectReferenceValue = card;
            });
            return view;
        }

        static CategoryTabView BuildTabTemplate(Transform parent, Font font, Sprite uiSprite)
        {
            var go = new GameObject("Category Tab Template", typeof(RectTransform), typeof(Image), typeof(Button), typeof(CategoryTabView));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(300f, 90f);

            var background = go.GetComponent<Image>();
            background.sprite = uiSprite;
            background.type = Image.Type.Sliced;
            var button = go.GetComponent<Button>();
            button.targetGraphic = background;

            var label = MakeText(go.transform, "Label", "", 34, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Fill(label.rectTransform, 0f, 1f);
            FitText(label, 18, 34);

            var tab = go.GetComponent<CategoryTabView>();
            Assign(tab, so =>
            {
                so.FindProperty("background").objectReferenceValue = background;
                so.FindProperty("label").objectReferenceValue = label;
                so.FindProperty("button").objectReferenceValue = button;
            });
            go.SetActive(false);
            return tab;
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

        // ---------- game prefabs: world + HUD + answer pad + controller in one self-contained prefab ----------

        /// <summary>Moves the game's scene objects into one prefab and removes them from the scene. Returns the prefab's Game.</summary>
        static Game Pack(GameSpec spec, Canvas mainCanvas, UnityEngine.SceneManagement.Scene scene, Font font, Sprite uiSprite)
        {
            GameObject root = null;
            foreach (var candidate in scene.GetRootGameObjects())
                if (candidate.name == spec.rootName) root = candidate;

            if (root == null)
            {
                var existing = AssetDatabase.LoadAssetAtPath<Game>(spec.prefabPath);
                if (existing == null)
                    Debug.LogWarning($"Menu: '{spec.rootName}' is not in the scene and {spec.prefabPath} does not exist - run its builder first.");
                return existing;
            }

            var oldCanvas = root.transform.Find("Game Canvas");
            if (oldCanvas != null) UnityEngine.Object.DestroyImmediate(oldCanvas.gameObject);

            var canvasObject = new GameObject("Game Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(root.transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0f;

            var hud = mainCanvas.transform.Find(spec.hudName);
            if (hud != null) hud.SetParent(canvasObject.transform, false);

            var pad = BuildAnswerPad(canvasObject.transform, font, uiSprite, spec.padColor);
            var game = root.GetComponent<Game>();
            Assign(game, so => so.FindProperty("answerPad").objectReferenceValue = pad);
            StyleHud(canvasObject.transform, uiSprite);

            Directory.CreateDirectory(Path.GetDirectoryName(spec.prefabPath));
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, spec.prefabPath);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab.GetComponent<Game>();
        }

        /// <summary>
        /// The sky, mountains, sun and ground used to be part of the Yurta game and were shared by Otara only because both
        /// lived in the scene. Now every game prefab carries its own "Scenery"; this copies it into prefabs that lack one.
        /// </summary>
        static void EnsureScenery(Game[] prefabs)
        {
            Transform source = null;
            foreach (var prefab in prefabs)
            {
                if (prefab == null) continue;
                source = prefab.transform.Find("Scenery");
                if (source != null) break;
            }
            if (source == null) return;

            foreach (var prefab in prefabs)
            {
                if (prefab == null || prefab.transform.Find("Scenery") != null) continue;

                string path = AssetDatabase.GetAssetPath(prefab);
                var contents = PrefabUtility.LoadPrefabContents(path);
                var scenery = UnityEngine.Object.Instantiate(source.gameObject, contents.transform);
                scenery.name = "Scenery";
                scenery.transform.SetAsFirstSibling();
                PrefabUtility.SaveAsPrefabAsset(contents, path);
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }

        static void AssignPrefabs(GameCatalog catalog, Game[] prefabs)
        {
            var so = new SerializedObject(catalog);
            var list = so.FindProperty("entries");
            for (int i = 0; i < list.arraySize; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                int number = item.FindPropertyRelative("menuNumber").intValue;
                for (int s = 0; s < Specs.Length; s++)
                    if (Specs[s].menuNumber == number && prefabs[s] != null)
                        item.FindPropertyRelative("prefab").objectReferenceValue = prefabs[s];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
        }

        static AnswerPadView BuildAnswerPad(Transform parent, Font font, Sprite uiSprite, Color buttonColor)
        {
            var padObject = new GameObject("Answer Pad", typeof(RectTransform), typeof(AnswerPadView));
            padObject.transform.SetParent(parent, false);
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
            template.GetComponent<Image>().color = buttonColor;
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

        static void StyleHud(Transform canvasRoot, Sprite uiSprite)
        {
            foreach (var text in canvasRoot.GetComponentsInChildren<Text>(true))
            {
                if (Array.IndexOf(HudTexts, text.name) < 0) continue;
                text.color = Color.white;
                text.fontStyle = FontStyle.Bold;
                AddBackdrop(text, uiSprite);
            }

            foreach (var image in canvasRoot.GetComponentsInChildren<Image>(true))
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

        /// <summary>Creates the catalog, or adds the categories and games it does not know yet. Never overwrites edits.</summary>
        static GameCatalog CreateCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<GameCatalog>();
                AssetDatabase.CreateAsset(catalog, CatalogPath);
            }

            var so = new SerializedObject(catalog);
            var categories = so.FindProperty("categories");
            for (int i = categories.arraySize; i < Categories.Length; i++)
            {
                categories.arraySize = i + 1;
                var item = categories.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("title").stringValue = Categories[i].title;
                item.FindPropertyRelative("accent").colorValue = Categories[i].accent;
            }

            var entries = so.FindProperty("entries");
            var known = new System.Collections.Generic.HashSet<int>();
            for (int i = 0; i < entries.arraySize; i++)
                known.Add(entries.GetArrayElementAtIndex(i).FindPropertyRelative("menuNumber").intValue);

            foreach (var game in CatalogGames)
            {
                if (known.Contains(game.number)) continue;

                int index = entries.arraySize;
                entries.arraySize = index + 1;
                var item = entries.GetArrayElementAtIndex(index);
                item.FindPropertyRelative("menuNumber").intValue = game.number;
                item.FindPropertyRelative("category").intValue = game.category;
                item.FindPropertyRelative("title").stringValue = game.title;
                item.FindPropertyRelative("description").stringValue = game.description;
                item.FindPropertyRelative("accent").colorValue = Categories[game.category].accent;
            }

            // growing an array copies the previous element, so a placeholder would inherit a real game's prefab
            for (int i = 0; i < entries.arraySize; i++)
            {
                var item = entries.GetArrayElementAtIndex(i);
                int number = item.FindPropertyRelative("menuNumber").intValue;
                if (!System.Array.Exists(Specs, spec => spec.menuNumber == number))
                    item.FindPropertyRelative("prefab").objectReferenceValue = null;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return catalog;
        }
    }
}
