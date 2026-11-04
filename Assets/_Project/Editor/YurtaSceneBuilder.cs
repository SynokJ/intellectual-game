using System;
using System.Collections.Generic;
using System.IO;
using IntellectualGame.Core;
using IntellectualGame.Yurta;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>
    /// Builds the Yurta beta in SampleScene: primitive sprites, level asset, HUD, win screen and all references.
    /// Runs once automatically after the first compile and can be re-run from Tools/Yurta/Build Scene.
    /// </summary>
    public static class YurtaSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string SpritesFolder = "Assets/_Project/MiniGames/Yurta/Art";
        const string LevelPath = "Assets/_Project/MiniGames/Yurta/Data/YurtaLevel01.asset";
        const string MarkerPath = "Library/YurtaSceneBuilt.txt";

        [MenuItem("Tools/Yurta/Build Scene")]
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
            Directory.CreateDirectory(Path.GetDirectoryName(LevelPath));
            Directory.CreateDirectory(SpritesFolder);

            var sprites = CreateSprites(SpritesFolder);
            CreateLevel();

            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            RemoveOld(scene);

            var canvas = FindMainCanvas();
            if (canvas == null) throw new InvalidOperationException("Canvas not found in SampleScene.");

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var uiSprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

            // HUD
            var hint = MakeText(canvas.transform, "Hint Text", "", 44, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1500f, 80f));

            var hud = new GameObject("Yurta HUD", typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)hud.transform);
            var equation = MakeText(hud.transform, "Equation Text", "", 110, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), new Vector2(-560f, 120f), new Vector2(560f, 200f), FontStyle.Bold);
            var progress = MakeText(hud.transform, "Progress Text", "", 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), new Vector2(-560f, -20f), new Vector2(560f, 60f));
            var feedback = MakeText(hud.transform, "Feedback Text", "", 44, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), new Vector2(-560f, -90f), new Vector2(560f, 70f), FontStyle.Bold);

            var quit = MakeButton(canvas.transform, "Quit Button", "Меню", font, uiSprite,
                new Vector2(0f, 1f), new Vector2(130f, -60f), new Vector2(220f, 80f));

            // Win screen (last sibling so it covers everything and blocks the number buttons)
            var win = new GameObject("Win Panel", typeof(RectTransform), typeof(Image));
            win.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)win.transform);
            win.GetComponent<Image>().color = new Color(0.05f, 0.08f, 0.15f, 0.75f);
            var winText = MakeText(win.transform, "Win Text", "", 90, Color.white, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), new Vector2(1400f, 360f), FontStyle.Bold);
            var replay = MakeButton(win.transform, "Replay Button", "Играть снова", font, uiSprite,
                new Vector2(0.5f, 0.5f), new Vector2(-230f, -170f), new Vector2(400f, 110f));
            var menu = MakeButton(win.transform, "Menu Button", "В меню", font, uiSprite,
                new Vector2(0.5f, 0.5f), new Vector2(230f, -170f), new Vector2(400f, 110f));
            win.SetActive(false);

            // World
            var world = BuildWorld(sprites, out var parts, out var tunduk);
            var game = world.AddComponent<YurtaGame>();
            var view = world.AddComponent<YurtaView>();

            Assign(game, so =>
            {
                so.FindProperty("level").objectReferenceValue = AssetDatabase.LoadAssetAtPath<YurtaLevelData>(LevelPath);
                so.FindProperty("view").objectReferenceValue = view;
            });
            Assign(view, so =>
            {
                so.FindProperty("hudRoot").objectReferenceValue = hud;
                so.FindProperty("equationText").objectReferenceValue = equation;
                so.FindProperty("progressText").objectReferenceValue = progress;
                so.FindProperty("feedbackText").objectReferenceValue = feedback;
                SetArray(so.FindProperty("parts"), parts);
                so.FindProperty("tunduk").objectReferenceValue = tunduk;
            });

            var controllerObject = new GameObject("GameController");
            var controller = controllerObject.AddComponent<GameController>();
            Assign(controller, so =>
            {
                so.FindProperty("hintText").objectReferenceValue = hint;
                so.FindProperty("quitButton").objectReferenceValue = quit;
                so.FindProperty("winPanel").objectReferenceValue = win;
                so.FindProperty("winText").objectReferenceValue = winText;
                so.FindProperty("replayButton").objectReferenceValue = replay;
                so.FindProperty("menuButton").objectReferenceValue = menu;
            });

            var camera = Camera.main;
            if (camera != null)
            {
                camera.backgroundColor = new Color(0.62f, 0.82f, 0.95f);
                camera.clearFlags = CameraClearFlags.SolidColor;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            EnsureInBuildSettings();
            File.WriteAllText(MarkerPath, DateTime.Now.ToString("o"));
            Debug.Log("Yurta scene built. Press Play and click button 1.");
        }

        static void RemoveOld(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "GameController" || root.name == "YurtaGame")
                    UnityEngine.Object.DestroyImmediate(root);

            var canvas = FindMainCanvas();
            if (canvas == null) return;
            foreach (var name in new[] { "Hint Text", "Yurta HUD", "Quit Button", "Win Panel" })
            {
                var old = canvas.transform.Find(name);
                if (old != null) UnityEngine.Object.DestroyImmediate(old.gameObject);
            }
        }

        // ---------- world (primitive shapes) ----------

        static GameObject BuildWorld(Dictionary<string, Sprite> s, out Transform[] parts, out Transform tunduk)
        {
            var world = new GameObject("YurtaGame");

            // scenery: always visible
            var scenery = Group("Scenery", world.transform, Vector2.zero);
            Shape(scenery, "Sun", s["Circle"], new Vector2(6.8f, 3.9f), new Vector2(1.6f, 1.6f), new Color(1f, 0.87f, 0.3f), -9);
            Shape(scenery, "Mountain Left", s["Triangle"], new Vector2(-5.2f, 1.7f), new Vector2(7f, 3.6f), new Color(0.5f, 0.58f, 0.7f), -8);
            Shape(scenery, "Mountain Right", s["Triangle"], new Vector2(5.4f, 2.1f), new Vector2(8f, 4.4f), new Color(0.42f, 0.5f, 0.64f), -8);
            Shape(scenery, "Snow Left", s["Triangle"], new Vector2(-5.2f, 3.0f), new Vector2(2.1f, 1.1f), Color.white, -7);
            Shape(scenery, "Snow Right", s["Triangle"], new Vector2(5.4f, 3.6f), new Vector2(2.4f, 1.3f), Color.white, -7);
            Shape(scenery, "Ground", s["Square"], new Vector2(0f, -2.6f), new Vector2(24f, 5f), new Color(0.42f, 0.67f, 0.31f), -6);

            var wall = new Color(0.61f, 0.42f, 0.25f);
            var wallDark = new Color(0.37f, 0.23f, 0.12f);
            var felt = new Color(0.96f, 0.92f, 0.86f);
            var red = new Color(0.78f, 0.16f, 0.16f);
            var gold = new Color(0.9f, 0.71f, 0.13f);

            // 1. kerege - lattice walls
            var kerege = Group("Part 1 Kerege", world.transform, new Vector2(0f, 0.8f));
            Shape(kerege, "Wall", s["Square"], new Vector2(0f, 0.8f), new Vector2(5f, 1.8f), wall, 1);
            for (int i = -3; i <= 3; i++)
                Shape(kerege, "Lattice " + i, s["Square"], new Vector2(i * 0.7f, 0.8f), new Vector2(0.55f, 0.55f), wallDark, 2, 45f);

            // 2. uuk - roof poles
            var uuk = Group("Part 2 Uuk", world.transform, new Vector2(0f, 2.5f));
            var top = new Vector2(0f, 3.4f);
            for (int i = -3; i <= 3; i++)
            {
                var bottom = new Vector2(i * 0.78f, 1.7f);
                var delta = top - bottom;
                Shape(uuk, "Pole " + i, s["Square"], (top + bottom) / 2f, new Vector2(0.07f, delta.magnitude),
                    new Color(0.48f, 0.3f, 0.15f), 3, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg - 90f);
            }

            // 3. kiyiz - felt cover (half circle stretched into a dome)
            var kiyiz = Group("Part 3 Kiyiz", world.transform, new Vector2(0f, 1.7f));
            Shape(kiyiz, "Dome", s["HalfCircle"], new Vector2(0f, 1.7f), new Vector2(5f, 3.8f), felt, 4);

            // 4. eshik - door
            var door = Group("Part 4 Door", world.transform, new Vector2(0f, 0.55f));
            Shape(door, "Door", s["Square"], new Vector2(0f, 0.55f), new Vector2(1f, 1.3f), new Color(0.56f, 0.11f, 0.11f), 6);
            Shape(door, "Door Frame", s["Square"], new Vector2(0f, 0.55f), new Vector2(0.78f, 1.1f), new Color(0.68f, 0.18f, 0.18f), 7);
            Shape(door, "Handle", s["Circle"], new Vector2(0.22f, 0.5f), new Vector2(0.14f, 0.14f), gold, 8);

            // 5. ornament band
            var band = Group("Part 5 Ornament", world.transform, new Vector2(0f, 1.55f));
            Shape(band, "Band", s["Square"], new Vector2(0f, 1.55f), new Vector2(5f, 0.26f), red, 5);
            for (int i = -6; i <= 6; i++)
                Shape(band, "Diamond " + i, s["Square"], new Vector2(i * 0.38f, 1.55f), new Vector2(0.13f, 0.13f), Color.white, 6, 45f);

            // reward: tunduk
            var tundukGroup = Group("Tunduk", world.transform, new Vector2(0f, 3.4f));
            Shape(tundukGroup, "Ring", s["Circle"], new Vector2(0f, 3.4f), new Vector2(0.95f, 0.95f), gold, 10);
            Shape(tundukGroup, "Hole", s["Circle"], new Vector2(0f, 3.4f), new Vector2(0.75f, 0.75f), new Color(0.17f, 0.23f, 0.33f), 11);
            Shape(tundukGroup, "Cross A", s["Square"], new Vector2(0f, 3.4f), new Vector2(0.75f, 0.07f), gold, 12);
            Shape(tundukGroup, "Cross B", s["Square"], new Vector2(0f, 3.4f), new Vector2(0.07f, 0.75f), gold, 12);
            Shape(tundukGroup, "Center", s["Circle"], new Vector2(0f, 3.4f), new Vector2(0.2f, 0.2f), gold, 13);

            parts = new[] { kerege, uuk, kiyiz, door, band };
            tunduk = tundukGroup;
            return world;
        }

        // ---------- level data ----------

        static YurtaLevelData CreateLevel()
        {
            var level = AssetDatabase.LoadAssetAtPath<YurtaLevelData>(LevelPath);
            if (level != null) return level;

            level = ScriptableObject.CreateInstance<YurtaLevelData>();
            AssetDatabase.CreateAsset(level, LevelPath);

            var so = new SerializedObject(level);
            var list = so.FindProperty("equations");
            var equations = new[]
            {
                (1, EquationOperator.Add, 1),
                (2, EquationOperator.Add, 3),
                (4, EquationOperator.Add, 2),
                (7, EquationOperator.Subtract, 3),
                (3, EquationOperator.Add, 4),
            };
            list.arraySize = equations.Length;
            for (int i = 0; i < equations.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("a").intValue = equations[i].Item1;
                item.FindPropertyRelative("op").enumValueIndex = (int)equations[i].Item2;
                item.FindPropertyRelative("b").intValue = equations[i].Item3;
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
            return level;
        }

        static void EnsureInBuildSettings()
        {
            foreach (var existing in EditorBuildSettings.scenes)
                if (existing.path == ScenePath) return;
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes)
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
