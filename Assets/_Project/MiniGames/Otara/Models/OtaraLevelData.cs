using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IntellectualGame.Otara
{
    public enum OtaraRoundKind { Count, Share }

    [Serializable]
    public struct OtaraRound
    {
        public OtaraRoundKind kind;
        [Tooltip("Count: sheep already in the pen. Share: total sheep to share.")]
        public int startCount;
        [Tooltip("Count only: positive = sheep enter the pen, negative = sheep leave it.")]
        public int[] moves;
        [Tooltip("Share only: number of families.")]
        public int families;

        public int Answer => kind == OtaraRoundKind.Count
            ? startCount + (moves == null ? 0 : moves.Sum())
            : startCount / Mathf.Max(1, families);

        public string Question => kind == OtaraRoundKind.Count
            ? "Сколько овец в загоне?"
            : $"Раздай поровну {families} семьям.\nСколько овец у каждой семьи?";
    }

    /// <summary>
    /// Model (config): the rounds of the game. Either a fixed list, or new random rounds generated at the start of every run:
    /// counting rounds (sheep enter and leave the pen) first, then sharing rounds (sheep go to 2 or 3 families).
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Otara/Level", fileName = "OtaraLevel")]
    public class OtaraLevelData : ScriptableObject
    {
        public const int MaxSheep = 7;

        const int GenerateAttempts = 30;

        [Tooltip("On: generate new random rounds every run. Off: use the fixed list below.")]
        [SerializeField] bool randomize = true;
        [Tooltip("Random: how many counting rounds (sheep enter and leave the pen).")]
        [SerializeField, Min(0)] int countRounds = 4;
        [Tooltip("Random: how many sharing rounds (sheep go to families), played after the counting rounds.")]
        [SerializeField, Min(0)] int shareRounds = 2;
        [SerializeField] List<OtaraRound> rounds = new List<OtaraRound>();

        public IReadOnlyList<OtaraRound> Rounds => rounds;

        /// <summary>The biggest answer the level can produce; the answer pad gets at least this many buttons.</summary>
        public int MaxAnswer => randomize ? MaxSheep : (rounds.Count == 0 ? 0 : rounds.Max(round => round.Answer));

        /// <summary>The rounds of one run: random ones, or a copy of the fixed list.</summary>
        public List<OtaraRound> CreateRun()
        {
            if (!randomize) return new List<OtaraRound>(rounds);

            var run = new List<OtaraRound>();
            for (int i = 0; i < countRounds; i++) run.Add(RandomCountRound());
            for (int i = 0; i < shareRounds; i++) run.Add(RandomShareRound());
            return run;
        }

        static OtaraRound RandomCountRound()
        {
            for (int attempt = 0; attempt < GenerateAttempts; attempt++)
            {
                int inPen = Random.Range(0, 6);
                var round = new OtaraRound
                {
                    kind = OtaraRoundKind.Count,
                    startCount = inPen,
                    moves = new int[Random.value < 0.6f ? 1 : 2],
                };

                for (int m = 0; m < round.moves.Length; m++)
                {
                    bool canAdd = inPen < MaxSheep;
                    bool canRemove = inPen > 0;
                    bool add = canAdd && (!canRemove || Random.value < 0.5f);
                    int move = add ? Random.Range(1, MaxSheep - inPen + 1) : -Random.Range(1, inPen + 1);
                    round.moves[m] = move;
                    inPen += move;
                }

                if (round.Answer >= 1) return round;
            }

            return new OtaraRound { kind = OtaraRoundKind.Count, startCount = 0, moves = new[] { 3 } };
        }

        static OtaraRound RandomShareRound()
        {
            int families = Random.value < 0.5f ? 2 : 3;
            int perFamily = families == 2 ? Random.Range(2, 5) : Random.Range(2, 4);
            return new OtaraRound { kind = OtaraRoundKind.Share, startCount = perFamily * families, moves = new int[0], families = families };
        }

        void OnValidate()
        {
            for (int i = 0; i < rounds.Count; i++)
            {
                var round = rounds[i];
                if (round.Answer < 1)
                    Debug.LogWarning($"{name}: round {i + 1} has answer {round.Answer}, but the buttons start at 1.", this);
                if (round.kind == OtaraRoundKind.Share && (round.families < 2 || round.families > 3 || round.startCount % round.families != 0))
                    Debug.LogWarning($"{name}: round {i + 1} must share evenly between 2 or 3 families.", this);

                int inPen = round.startCount;
                if (round.kind == OtaraRoundKind.Count && round.moves != null)
                    foreach (int move in round.moves)
                    {
                        inPen += move;
                        if (inPen < 0 || inPen > MaxSheep)
                            Debug.LogWarning($"{name}: round {i + 1} needs {inPen} sheep in the pen (allowed 0-{MaxSheep}).", this);
                    }
            }
        }
    }
}
