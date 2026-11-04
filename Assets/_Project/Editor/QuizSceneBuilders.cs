using IntellectualGame.Core;
using IntellectualGame.Manas;
using IntellectualGame.Proverb;
using UnityEditor;
using UnityEngine;
using static IntellectualGame.EditorTools.BuilderUtils;

namespace IntellectualGame.EditorTools
{
    /// <summary>Builds the two multiple-choice games (Manas road, Proverb banner). Both run on the shared QuizGame controller.</summary>
    internal static class QuizSceneBuilders
    {
        internal const string ManasPrefabPath = "Assets/_Project/MiniGames/Manas/Prefabs/ManasGame.prefab";
        internal const string ProverbPrefabPath = "Assets/_Project/MiniGames/Proverb/Prefabs/ProverbGame.prefab";
        const string ManasLevelPath = "Assets/_Project/MiniGames/Manas/Data/ManasQuiz01.asset";
        const string ProverbLevelPath = "Assets/_Project/MiniGames/Proverb/Data/ProverbQuiz01.asset";

        // (question, correct, wrong answers, note)
        static readonly (string text, string correct, string[] wrong, string note)[] ManasQuestions =
        {
            ("Как зовут коня Манаса?", "Ак-Кула", new[] { "Тулпар", "Аргымак", "Кара-Жорго" }, "Ак-Кула — верный конь богатыря Манаса."),
            ("Как зовут жену Манаса?", "Каныкей", new[] { "Айчүрөк", "Бермет", "Зарина" }, "Каныкей — мудрая жена Манаса."),
            ("Сколько частей в трилогии о Манасе, Семетее и Сейтеке?", "3", new[] { "2", "4", "5" }, "Трилогия: «Манас», «Семетей», «Сейтек»."),
            ("Как называют сказителей эпоса «Манас»?", "Манасчы", new[] { "Комузчу", "Акын", "Чабан" }, "Манасчы исполняют эпос наизусть."),
            ("Как зовут сына Манаса?", "Семетей", new[] { "Сейтек", "Бакай", "Алмамбет" }, "Сын Манаса — Семетей, а внук — Сейтек."),
            ("Кто был мудрым советником Манаса?", "Бакай", new[] { "Семетей", "Сейтек", "Каныкей" }, "Бакай — мудрый советник и друг Манаса."),
        };

        static readonly (string text, string correct, string[] wrong, string note)[] ProverbQuestions =
        {
            ("Допиши пословицу:\nАт — жигиттин ___", "канаты", new[] { "көзү", "жолу", "тили" }, "Конь — крылья джигита."),
            ("Допиши пословицу:\nЖети өлчөп, бир ___", "кес", new[] { "бер", "ал", "айт" }, "Семь раз отмерь, один раз отрежь."),
            ("Допиши пословицу:\nЖакшы сөз — жарым ___", "ырыс", new[] { "өмүр", "байлык", "дос" }, "Доброе слово — половина счастья."),
            ("Допиши пословицу:\nБирдик болбой, ___ болбойт", "тирдик", new[] { "бүтүндүк", "байлык", "тынчтык" }, "Без единства не бывает жизни."),
        };

        [MenuItem("Tools/Manas/Rebuild Game")]
        static void RebuildManas()
        {
            if (System.IO.File.Exists(ManasPrefabPath)) AssetDatabase.DeleteAsset(ManasPrefabPath);
            MenuSceneBuilder.Build();
        }

        [MenuItem("Tools/Proverb/Rebuild Game")]
        static void RebuildProverb()
        {
            if (System.IO.File.Exists(ProverbPrefabPath)) AssetDatabase.DeleteAsset(ProverbPrefabPath);
            MenuSceneBuilder.Build();
        }

        internal static void AddManas(Canvas mainCanvas)
        {
            System.IO.Directory.CreateDirectory(SharedSpritesFolder);
            var s = CreateSprites(SharedSpritesFolder);
            var level = QuizLevel(ManasLevelPath, "Манас: путь героя", "Герой дошёл до юрты!", ManasQuestions);

            RemoveOld("ManasGame", "Manas HUD", mainCanvas);
            var hud = StandardHud(mainCanvas, "Manas HUD", AppFont(), true);

            var world = new GameObject("ManasGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var stage = stageObject.transform;

            Shape(stage, "Road", s["Square"], new Vector2(-0.5f, -2.6f), new Vector2(14f, 0.3f), new Color(0.78f, 0.65f, 0.45f), -4);
            var roadStart = Anchor(stage, "Road Start", -7f, -2.55f);
            var roadEnd = Anchor(stage, "Road End", 3.8f, -2.55f);

            // the rider; the pivot is at the horse's feet
            var hero = Group("Hero", stage, new Vector2(-7f, -2.55f));
            var brown = new Color(0.45f, 0.28f, 0.15f);
            Shape(hero, "Leg Back", s["Square"], new Vector2(-7.4f, -2.2f), new Vector2(0.14f, 0.7f), brown, 1);
            Shape(hero, "Leg Front", s["Square"], new Vector2(-6.6f, -2.2f), new Vector2(0.14f, 0.7f), brown, 1);
            Shape(hero, "Horse", s["Circle"], new Vector2(-7f, -1.85f), new Vector2(1.5f, 0.8f), brown, 2);
            Shape(hero, "Horse Head", s["Circle"], new Vector2(-6.2f, -1.5f), new Vector2(0.5f, 0.55f), brown, 2);
            Shape(hero, "Rider Body", s["Square"], new Vector2(-7f, -1.25f), new Vector2(0.4f, 0.7f), new Color(0.8f, 0.2f, 0.2f), 3);
            Shape(hero, "Rider Head", s["Circle"], new Vector2(-7f, -0.75f), new Vector2(0.38f, 0.38f), new Color(0.95f, 0.8f, 0.65f), 3);
            Shape(hero, "Rider Hat", s["HalfCircle"], new Vector2(-7f, -0.68f), new Vector2(0.42f, 0.5f), Color.white, 4);

            // the finish: a small yurt
            var finish = Group("Finish", stage, new Vector2(6.2f, -2.55f));
            Shape(finish, "Dome", s["HalfCircle"], new Vector2(6.2f, -1.9f), new Vector2(2.4f, 2f), new Color(0.96f, 0.92f, 0.86f), 1);
            Shape(finish, "Wall", s["Square"], new Vector2(6.2f, -2.2f), new Vector2(2.4f, 0.7f), new Color(0.61f, 0.42f, 0.25f), 2);
            Shape(finish, "Door", s["Square"], new Vector2(6.2f, -2.2f), new Vector2(0.5f, 0.6f), new Color(0.56f, 0.11f, 0.11f), 3);
            Shape(finish, "Tunduk", s["Circle"], new Vector2(6.2f, -0.95f), new Vector2(0.4f, 0.4f), new Color(0.9f, 0.71f, 0.13f), 3);

            var view = world.AddComponent<ManasStageView>();
            Assign(view, so =>
            {
                so.FindProperty("stage").objectReferenceValue = stageObject;
                so.FindProperty("hero").objectReferenceValue = hero;
                so.FindProperty("roadStart").objectReferenceValue = roadStart;
                so.FindProperty("roadEnd").objectReferenceValue = roadEnd;
                so.FindProperty("finish").objectReferenceValue = finish;
            });
            Wire(world, level, hud, view);
            stageObject.SetActive(false);
        }

        internal static void AddProverb(Canvas mainCanvas)
        {
            System.IO.Directory.CreateDirectory(SharedSpritesFolder);
            var s = CreateSprites(SharedSpritesFolder);
            var level = QuizLevel(ProverbLevelPath, "Ребус-пословица", "Вся народная мудрость собрана!", ProverbQuestions);

            RemoveOld("ProverbGame", "Proverb HUD", mainCanvas);
            var hud = StandardHud(mainCanvas, "Proverb HUD", AppFont(), true);

            var world = new GameObject("ProverbGame");
            var stageObject = new GameObject("Stage");
            stageObject.transform.SetParent(world.transform, false);
            var stage = stageObject.transform;

            // a felt banner with a red trim; gems are cloned from the template
            Shape(stage, "Banner", s["Square"], new Vector2(0f, -2.2f), new Vector2(11f, 1.1f), new Color(0.96f, 0.92f, 0.86f), -4);
            Shape(stage, "Trim Top", s["Square"], new Vector2(0f, -1.67f), new Vector2(11f, 0.1f), new Color(0.78f, 0.2f, 0.2f), -3);
            Shape(stage, "Trim Bottom", s["Square"], new Vector2(0f, -2.73f), new Vector2(11f, 0.1f), new Color(0.78f, 0.2f, 0.2f), -3);
            var anchor = Anchor(stage, "Gem Anchor", 0f, -2.2f);
            var template = new GameObject("Gem Template", typeof(SpriteRenderer));
            template.transform.SetParent(stage, false);
            template.transform.position = new Vector3(0f, -2.2f, 0f);
            template.transform.rotation = Quaternion.Euler(0f, 0f, 45f);
            template.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            var gem = template.GetComponent<SpriteRenderer>();
            gem.sprite = s["Square"];
            gem.sortingOrder = 1;
            template.SetActive(false);

            var view = world.AddComponent<ProverbStageView>();
            Assign(view, so =>
            {
                so.FindProperty("stage").objectReferenceValue = stageObject;
                so.FindProperty("gemTemplate").objectReferenceValue = gem;
                so.FindProperty("gemAnchor").objectReferenceValue = anchor;
            });
            Wire(world, level, hud, view);
            stageObject.SetActive(false);
        }

        static void Wire(GameObject world, QuizLevelData level, HudParts hud, QuizStageView stage)
        {
            var game = world.AddComponent<QuizGame>();
            var hudView = world.AddComponent<GameHudView>();
            Assign(game, so =>
            {
                so.FindProperty("level").objectReferenceValue = level;
                so.FindProperty("hud").objectReferenceValue = hudView;
                so.FindProperty("stage").objectReferenceValue = stage;
            });
            WireHud(hudView, hud);
        }

        static QuizLevelData QuizLevel(string path, string title, string winMessage,
            (string text, string correct, string[] wrong, string note)[] questions)
        {
            return EnsureAsset<QuizLevelData>(path, so =>
            {
                so.FindProperty("title").stringValue = title;
                so.FindProperty("winMessage").stringValue = winMessage;
                so.FindProperty("questionCount").intValue = Mathf.Min(5, questions.Length);

                var list = so.FindProperty("questions");
                list.arraySize = questions.Length;
                for (int i = 0; i < questions.Length; i++)
                {
                    var item = list.GetArrayElementAtIndex(i);
                    item.FindPropertyRelative("text").stringValue = questions[i].text;
                    item.FindPropertyRelative("correct").stringValue = questions[i].correct;
                    item.FindPropertyRelative("note").stringValue = questions[i].note;
                    var wrong = item.FindPropertyRelative("wrong");
                    wrong.arraySize = questions[i].wrong.Length;
                    for (int w = 0; w < questions[i].wrong.Length; w++)
                        wrong.GetArrayElementAtIndex(w).stringValue = questions[i].wrong[w];
                }
            });
        }
    }
}
