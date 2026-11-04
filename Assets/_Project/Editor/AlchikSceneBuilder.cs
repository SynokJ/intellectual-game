using IntellectualGame.Alchik;
using IntellectualGame.Core;
using UnityEditor;
using UnityEngine;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>Builds the Alchik game (thrower, angle arrow, knucklebone, target) from primitives; the menu builder packs it into a prefab.</summary>
    internal static class AlchikSceneBuilder
    {
        internal const string PrefabPath = "Assets/_Project/MiniGames/Alchik/Prefabs/AlchikGame.prefab";
        const string LevelPath = "Assets/_Project/MiniGames/Alchik/Data/AlchikLevel01.asset";

        [MenuItem("Tools/Alchik/Rebuild Game")]
        static void Rebuild()
        {
            if (System.IO.File.Exists(PrefabPath)) AssetDatabase.DeleteAsset(PrefabPath);
            MenuSceneBuilder.Build();
        }

        internal static void AddToScene(Canvas mainCanvas)
        {
            System.IO.Directory.CreateDirectory(SharedSpritesFolder);
            var s = CreateSprites(SharedSpritesFolder);
            var level = EnsureAsset<AlchikLevelData>(LevelPath);
            RemoveOld("AlchikGame", "Alchik HUD", mainCanvas);
            var hud = StandardHud(mainCanvas, "Alchik HUD", Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"), false);

            var world = new GameObject("AlchikGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var stage = stageObject.transform;

            var wood = new Color(0.45f, 0.3f, 0.15f);
            for (int i = 0; i <= 14; i++)
                Shape(stage, "Tick " + i, s["Square"], new Vector2(-7.2f + i, -1.85f), new Vector2(0.06f, i % 5 == 0 ? 0.4f : 0.22f), wood, 2);

            // the thrower
            var thrower = Group("Thrower", stage, Vector2.zero);
            Shape(thrower, "Robe", s["HalfCircle"], new Vector2(-7.9f, -2.1f), new Vector2(1f, 1.9f), new Color(0.25f, 0.5f, 0.75f), 3);
            Shape(thrower, "Head", s["Circle"], new Vector2(-7.9f, -0.95f), new Vector2(0.55f, 0.55f), new Color(0.95f, 0.8f, 0.65f), 4);
            Shape(thrower, "Hat", s["HalfCircle"], new Vector2(-7.9f, -0.8f), new Vector2(0.6f, 0.7f), Color.white, 5);

            var start = Anchor(stage, "Start Point", -7.2f, -1.4f);

            var arrow = Group("Angle Arrow", stage, new Vector2(-7.2f, -1.4f));
            Shape(arrow, "Shaft", s["Square"], new Vector2(-6.4f, -1.4f), new Vector2(1.6f, 0.08f), new Color(0.9f, 0.3f, 0.2f), 6);
            Shape(arrow, "Head", s["Triangle"], new Vector2(-5.65f, -1.4f), new Vector2(0.3f, 0.3f), new Color(0.9f, 0.3f, 0.2f), 6, -90f);

            var target = Group("Target", stage, new Vector2(0f, -1.4f));
            Shape(target, "Pole", s["Square"], new Vector2(0f, -0.8f), new Vector2(0.05f, 1.2f), wood, 4);
            Shape(target, "Ring", s["Circle"], new Vector2(0f, -1.4f), Vector2.one, new Color(0.8f, 0.2f, 0.2f), 5);
            Shape(target, "Ring White", s["Circle"], new Vector2(0f, -1.4f), new Vector2(0.62f, 0.62f), Color.white, 6);
            Shape(target, "Bullseye", s["Circle"], new Vector2(0f, -1.4f), new Vector2(0.25f, 0.25f), new Color(0.8f, 0.2f, 0.2f), 7);

            var bone = Group("Alchik", stage, new Vector2(-7.2f, -1.4f));
            var cream = new Color(0.97f, 0.92f, 0.8f);
            Shape(bone, "Body", s["Square"], new Vector2(-7.2f, -1.4f), new Vector2(0.5f, 0.2f), cream, 20);
            Shape(bone, "Knob Left", s["Circle"], new Vector2(-7.46f, -1.4f), new Vector2(0.24f, 0.24f), cream, 20);
            Shape(bone, "Knob Right", s["Circle"], new Vector2(-6.94f, -1.4f), new Vector2(0.24f, 0.24f), cream, 20);

            var game = world.AddComponent<AlchikGame>();
            var view = world.AddComponent<AlchikView>();
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
                so.FindProperty("startPoint").objectReferenceValue = start;
                so.FindProperty("alchik").objectReferenceValue = bone;
                so.FindProperty("angleArrow").objectReferenceValue = arrow;
                so.FindProperty("target").objectReferenceValue = target;
            });
            WireHud(hudView, hud);
            stageObject.SetActive(false);
        }
    }
}
