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
    /// Builds the adaptive main menu in SampleScene: catalog asset, "Menu Canvas" with game cards and wiring into the GameController.
    /// Runs once automatically after the first compile and can be re-run from Tools/Menu/Build Menu.
    /// </summary>
    public static class MenuSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string CatalogPath = "Assets/_Project/Menu/Data/GameCatalog.asset";
        const string CirclePath = "Assets/_Project/MiniGames/Yurta/Art/Circle.png";
        const string MarkerPath = "Library/MenuSceneBuilt.txt";

        static readonly Color Sky = new Color(0.62f, 0.82f, 0.95f);

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
            if (controller == null || FindMainCanvas() == null)
            {
                Debug.LogError("Menu: build the game scenes first (Tools/Yurta/Build Scene, Tools/Otara/Build Scene) - GameController is missing.");
                return;
            }

            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "Menu Canvas") UnityEngine.Object.DestroyImmediate(root);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            var circle = AssetDatabase.LoadAssetAtPath<Sprite>(CirclePath);

            // canvas: its own scaler so the menu can switch between landscape and portrait references
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

            // opaque background covers the game world, content stays inside the safe area
            var panel = new GameObject("Menu Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(canvasObject.transform, false);
            Stretch((RectTransform)panel.transform);
            panel.GetComponent<Image>().color = Sky;

            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaFitter));
            safe.transform.SetParent(panel.transform, false);
            Stretch((RectTransform)safe.transform);

            var title = MakeText(safe.transform, "Title Text", "", 84, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            PinToTop(title.rectTransform, -40f, 130f);
            FitText(title, 40, 84);

            var subtitle = MakeText(safe.transform, "Subtitle Text", "", 44, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), Vector2.zero, Vector2.zero);
            PinToTop(subtitle.rectTransform, -175f, 70f);
            FitText(subtitle, 24, 44);

            // scrollable, adaptive grid of cards
            var scrollObject = new GameObject("Scroll View", typeof(RectTransform), typeof(ScrollRect));
            scrollObject.transform.SetParent(safe.transform, false);
            var scrollRect = (RectTransform)scrollObject.transform;
            Stretch(scrollRect);
            scrollRect.offsetMin = new Vector2(40f, 40f);
            scrollRect.offsetMax = new Vector2(-40f, -270f);

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
            contentRect.offsetMin = new Vector2(0f, 0f);
            contentRect.offsetMax = new Vector2(0f, 0f);
            var grid = content.GetComponent<GridLayoutGroup>();
            grid.padding = new RectOffset(20, 20, 20, 20);
            grid.spacing = new Vector2(40f, 40f);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

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

            // wire into the controller
            var numberButtons = new SerializedObject(controller).FindProperty("numberButtons");
            var firstButton = numberButtons.arraySize > 0
                ? numberButtons.GetArrayElementAtIndex(0).objectReferenceValue as Button : null;
            if (firstButton == null)
            {
                Debug.LogError("Menu: GameController has no number buttons - rebuild the game scenes first.");
                return;
            }

            var games = UnityEngine.Object.FindObjectsByType<Game>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            Assign(controller, so =>
            {
                so.FindProperty("catalog").objectReferenceValue = catalog;
                so.FindProperty("menuView").objectReferenceValue = view;
                SetArray(so.FindProperty("games"), games);
                so.FindProperty("answerPad").objectReferenceValue = firstButton.transform.parent.gameObject;
            });

            var camera = Camera.main;
            if (camera != null && camera.GetComponent<CameraFit>() == null) camera.gameObject.AddComponent<CameraFit>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, DateTime.Now.ToString("o"));
            Debug.Log("Main menu built. Press Play.");
        }

        static GameCardView BuildCardTemplate(Transform parent, Font font, Sprite uiSprite, Sprite circle)
        {
            var go = new GameObject("Game Card Template", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(CanvasGroup), typeof(GameCardView));
            go.transform.SetParent(parent, false);
            ((RectTransform)go.transform).sizeDelta = new Vector2(560f, 420f);

            var background = go.GetComponent<Image>();
            background.sprite = uiSprite;
            background.type = Image.Type.Sliced;
            var button = go.GetComponent<Button>();
            button.targetGraphic = background;

            // number badge, top left
            var badge = new GameObject("Number Badge", typeof(RectTransform), typeof(Image));
            badge.transform.SetParent(go.transform, false);
            var badgeRect = (RectTransform)badge.transform;
            badgeRect.anchorMin = badgeRect.anchorMax = badgeRect.pivot = new Vector2(0f, 1f);
            badgeRect.anchoredPosition = new Vector2(28f, -28f);
            badgeRect.sizeDelta = new Vector2(84f, 84f);
            var badgeImage = badge.GetComponent<Image>();
            badgeImage.sprite = circle;
            badgeImage.color = new Color(1f, 1f, 1f, 0.7f);
            badgeImage.raycastTarget = false;
            var number = MakeText(badge.transform, "Number Text", "", 48, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, badgeRect.sizeDelta, FontStyle.Bold);
            Stretch(number.rectTransform);

            // "coming soon" badge, top right
            var soon = new GameObject("Soon Badge", typeof(RectTransform), typeof(Image));
            soon.transform.SetParent(go.transform, false);
            var soonRect = (RectTransform)soon.transform;
            soonRect.anchorMin = soonRect.anchorMax = soonRect.pivot = new Vector2(1f, 1f);
            soonRect.anchoredPosition = new Vector2(-28f, -36f);
            soonRect.sizeDelta = new Vector2(170f, 60f);
            var soonImage = soon.GetComponent<Image>();
            soonImage.sprite = uiSprite;
            soonImage.type = Image.Type.Sliced;
            soonImage.color = Ink;
            soonImage.raycastTarget = false;
            var soonText = MakeText(soon.transform, "Soon Text", "Скоро", 32, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, soonRect.sizeDelta, FontStyle.Bold);
            Stretch(soonText.rectTransform);
            soon.SetActive(false);

            var title = MakeText(go.transform, "Title Text", "", 56, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero, FontStyle.Bold);
            Fill(title.rectTransform, 0.40f, 0.72f);
            FitText(title, 26, 56);

            var description = MakeText(go.transform, "Description Text", "", 34, Ink, TextAnchor.UpperCenter, font,
                new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Fill(description.rectTransform, 0.06f, 0.40f);
            FitText(description, 18, 34);

            var card = go.GetComponent<GameCardView>();
            Assign(card, so =>
            {
                so.FindProperty("background").objectReferenceValue = background;
                so.FindProperty("numberText").objectReferenceValue = number;
                so.FindProperty("titleText").objectReferenceValue = title;
                so.FindProperty("descriptionText").objectReferenceValue = description;
                so.FindProperty("soonBadge").objectReferenceValue = soon;
                so.FindProperty("button").objectReferenceValue = button;
                so.FindProperty("group").objectReferenceValue = go.GetComponent<CanvasGroup>();
            });
            go.SetActive(false);
            return card;
        }

        static void PinToTop(RectTransform rect, float y, float height)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = new Vector2(-80f, height);
        }

        static void Fill(RectTransform rect, float minY, float maxY)
        {
            rect.anchorMin = new Vector2(0f, minY);
            rect.anchorMax = new Vector2(1f, maxY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(30f, 0f);
            rect.offsetMax = new Vector2(-30f, 0f);
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
                (1, "Юрта: сборка", "Реши примеры и собери юрту", new Color(0.99f, 0.85f, 0.55f)),
                (2, "Отара овец", "Считай овец в загоне и раздай их семьям", new Color(0.72f, 0.9f, 0.68f)),
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
