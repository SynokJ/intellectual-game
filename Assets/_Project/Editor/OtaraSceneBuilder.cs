using System;
using System.IO;
using IntellectualGame.Otara;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>
    /// Adds the Otara (flock of sheep) beta to SampleScene next to the Yurta game: primitive sprites, level asset,
    /// HUD and references. Runs once automatically after the first compile; re-run from Tools/Otara/Build Scene.
    /// </summary>
    public static class OtaraSceneBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        const string SpritesFolder = "Assets/_Project/MiniGames/Otara/Art";
        const string LevelPath = "Assets/_Project/MiniGames/Otara/Data/OtaraLevel01.asset";
        const string MarkerPath = "Library/OtaraSceneBuilt.txt";

        [MenuItem("Tools/Otara/Build Scene")]
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
            var canvas = FindMainCanvas();
            var win = canvas != null ? canvas.transform.Find("Win Panel") : null;
            if (win == null || UnityEngine.Object.FindFirstObjectByType<IntellectualGame.Core.GameController>() == null)
            {
                Debug.LogError("Otara: build the Yurta scene first (Tools/Yurta/Build Scene) - GameController or Win Panel is missing.");
                return;
            }

            RemoveOld(scene, canvas);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // HUD
            var hud = new GameObject("Otara HUD", typeof(RectTransform));
            hud.transform.SetParent(canvas.transform, false);
            Stretch((RectTransform)hud.transform);
            var question = MakeText(hud.transform, "Question Text", "", 54, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(1400f, 140f), FontStyle.Bold);
            var events = MakeText(hud.transform, "Event Text", "", 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -300f), new Vector2(1200f, 60f));
            var feedback = MakeText(hud.transform, "Feedback Text", "", 44, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(0.5f, 1f), new Vector2(0f, -365f), new Vector2(1200f, 70f), FontStyle.Bold);
            var progress = MakeText(hud.transform, "Progress Text", "", 40, Ink, TextAnchor.MiddleCenter, font,
                new Vector2(1f, 1f), new Vector2(-190f, -60f), new Vector2(340f, 60f));
            hud.SetActive(false);
            win.SetAsLastSibling();

            // World
            var world = new GameObject("OtaraGame");
            var stage = new GameObject("Stage");
            stage.transform.SetParent(world.transform, false);

            var pen = BuildPen(stage.transform, sprites);
            var families = new Transform[3];
            for (int i = 0; i < families.Length; i++)
                families[i] = BuildFamilyYurt(stage.transform, sprites, i + 1);
            var sheep = BuildSheepTemplate(stage.transform, sprites);

            var game = world.AddComponent<OtaraGame>();
            var view = world.AddComponent<OtaraView>();
            stage.SetActive(false);

            Assign(game, so =>
            {
                so.FindProperty("level").objectReferenceValue = AssetDatabase.LoadAssetAtPath<OtaraLevelData>(LevelPath);
                so.FindProperty("view").objectReferenceValue = view;
            });
            Assign(view, so =>
            {
                so.FindProperty("stage").objectReferenceValue = stage;
                so.FindProperty("pen").objectReferenceValue = pen;
                so.FindProperty("hudRoot").objectReferenceValue = hud;
                so.FindProperty("sheepTemplate").objectReferenceValue = sheep;
                SetArray(so.FindProperty("familyYurts"), families);
                so.FindProperty("questionText").objectReferenceValue = question;
                so.FindProperty("eventText").objectReferenceValue = events;
                so.FindProperty("feedbackText").objectReferenceValue = feedback;
                so.FindProperty("progressText").objectReferenceValue = progress;
            });

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            File.WriteAllText(MarkerPath, DateTime.Now.ToString("o"));
            Debug.Log("Otara scene built. Press Play and click button 2.");
        }

        static void RemoveOld(UnityEngine.SceneManagement.Scene scene, Canvas canvas)
        {
            foreach (var root in scene.GetRootGameObjects())
                if (root.name == "OtaraGame") UnityEngine.Object.DestroyImmediate(root);

            var hud = canvas.transform.Find("Otara HUD");
            if (hud != null) UnityEngine.Object.DestroyImmediate(hud.gameObject);
        }

        // ---------- world (primitive shapes) ----------

        static GameObject BuildPen(Transform parent, System.Collections.Generic.Dictionary<string, Sprite> s)
        {
            var wood = new Color(0.55f, 0.38f, 0.22f);
            var pen = Group("Pen", parent, Vector2.zero);

            Shape(pen, "Dirt", s["Square"], new Vector2(2.15f, 0.4f), new Vector2(6.2f, 1.3f), new Color(0.72f, 0.6f, 0.42f), 1);
            for (int i = 0; i < 7; i++)
                Shape(pen, "Post " + i, s["Square"], new Vector2(-0.9f + 1.03f * i, 0.85f), new Vector2(0.14f, 1.7f), wood, 2);
            Shape(pen, "Rail Low", s["Square"], new Vector2(2.15f, 1.1f), new Vector2(6.2f, 0.1f), wood, 3);
            Shape(pen, "Rail High", s["Square"], new Vector2(2.15f, 1.55f), new Vector2(6.2f, 0.1f), wood, 3);

            // gate frame on the left
            var red = new Color(0.78f, 0.16f, 0.16f);
            foreach (float x in new[] { -1.15f, -0.75f })
            {
                Shape(pen, "Gate Post", s["Square"], new Vector2(x, 1.1f), new Vector2(0.2f, 2.0f), wood, 4);
                Shape(pen, "Gate Cap", s["Square"], new Vector2(x, 2.15f), new Vector2(0.28f, 0.28f), red, 5, 45f);
            }

            return pen.gameObject;
        }

        static Transform BuildFamilyYurt(Transform parent, System.Collections.Generic.Dictionary<string, Sprite> s, int number)
        {
            var yurt = Group("Family " + number, parent, Vector2.zero);
            var group = yurt.gameObject.AddComponent<SortingGroup>();
            group.sortingOrder = 50;

            Shape(yurt, "Dome", s["HalfCircle"], new Vector2(0f, 0.6f), new Vector2(1.6f, 1.7f), new Color(0.96f, 0.92f, 0.86f), 1);
            Shape(yurt, "Wall", s["Square"], new Vector2(0f, 0.3f), new Vector2(1.6f, 0.6f), new Color(0.61f, 0.42f, 0.25f), 2);
            Shape(yurt, "Band", s["Square"], new Vector2(0f, 0.58f), new Vector2(1.6f, 0.1f), new Color(0.78f, 0.16f, 0.16f), 3);
            Shape(yurt, "Door", s["Square"], new Vector2(0f, 0.22f), new Vector2(0.35f, 0.44f), new Color(0.56f, 0.11f, 0.11f), 4);
            Shape(yurt, "Tunduk", s["Circle"], new Vector2(0f, 1.42f), new Vector2(0.3f, 0.3f), new Color(0.9f, 0.71f, 0.13f), 5);

            yurt.gameObject.SetActive(false);
            return yurt;
        }

        static SheepView BuildSheepTemplate(Transform parent, System.Collections.Generic.Dictionary<string, Sprite> s)
        {
            var root = Group("Sheep Template", parent, Vector2.zero);
            var wool = new Color(0.97f, 0.95f, 0.9f);
            var dark = new Color(0.25f, 0.22f, 0.2f);

            Shape(root, "Leg Back", s["Square"], new Vector2(-0.25f, 0.2f), new Vector2(0.12f, 0.4f), dark, 0);
            Shape(root, "Leg Front", s["Square"], new Vector2(0.25f, 0.2f), new Vector2(0.12f, 0.4f), dark, 0);
            Shape(root, "Tail", s["Circle"], new Vector2(-0.55f, 0.72f), new Vector2(0.22f, 0.22f), wool, 0);
            Shape(root, "Body", s["Circle"], new Vector2(0f, 0.65f), new Vector2(1.05f, 0.8f), new Color(0.93f, 0.9f, 0.82f), 1);
            Shape(root, "Wool Left", s["Circle"], new Vector2(-0.28f, 1.0f), new Vector2(0.46f, 0.46f), wool, 2);
            Shape(root, "Wool Middle", s["Circle"], new Vector2(0f, 1.08f), new Vector2(0.5f, 0.5f), wool, 2);
            Shape(root, "Wool Right", s["Circle"], new Vector2(0.28f, 1.0f), new Vector2(0.46f, 0.46f), wool, 2);
            Shape(root, "Ear", s["Circle"], new Vector2(0.5f, 0.98f), new Vector2(0.3f, 0.14f), dark, 2, 25f);
            Shape(root, "Head", s["Circle"], new Vector2(0.62f, 0.72f), new Vector2(0.42f, 0.44f), dark, 3);
            Shape(root, "Eye", s["Circle"], new Vector2(0.72f, 0.78f), new Vector2(0.11f, 0.11f), Color.white, 4);
            Shape(root, "Pupil", s["Circle"], new Vector2(0.74f, 0.78f), new Vector2(0.05f, 0.05f), Color.black, 5);

            var sheep = root.gameObject.AddComponent<SheepView>();
            root.gameObject.SetActive(false);
            return sheep;
        }

        // ---------- level data ----------

        static void CreateLevel()
        {
            if (AssetDatabase.LoadAssetAtPath<OtaraLevelData>(LevelPath) != null) return;

            var level = ScriptableObject.CreateInstance<OtaraLevelData>();
            AssetDatabase.CreateAsset(level, LevelPath);

            var rounds = new (OtaraRoundKind kind, int start, int[] moves, int families)[]
            {
                (OtaraRoundKind.Count, 0, new[] { 3 }, 0),
                (OtaraRoundKind.Count, 2, new[] { 4 }, 0),
                (OtaraRoundKind.Count, 7, new[] { -3 }, 0),
                (OtaraRoundKind.Count, 3, new[] { 4, -2 }, 0),
                (OtaraRoundKind.Share, 6, new int[0], 2),
                (OtaraRoundKind.Share, 6, new int[0], 3),
            };

            var so = new SerializedObject(level);
            var list = so.FindProperty("rounds");
            list.arraySize = rounds.Length;
            for (int i = 0; i < rounds.Length; i++)
            {
                var item = list.GetArrayElementAtIndex(i);
                item.FindPropertyRelative("kind").enumValueIndex = (int)rounds[i].kind;
                item.FindPropertyRelative("startCount").intValue = rounds[i].start;
                item.FindPropertyRelative("families").intValue = rounds[i].families;
                var moves = item.FindPropertyRelative("moves");
                moves.arraySize = rounds[i].moves.Length;
                for (int m = 0; m < rounds[i].moves.Length; m++)
                    moves.GetArrayElementAtIndex(m).intValue = rounds[i].moves[m];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(level);
            AssetDatabase.SaveAssets();
        }
    }
}
