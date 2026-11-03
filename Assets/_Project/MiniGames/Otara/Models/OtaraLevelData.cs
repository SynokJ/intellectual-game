using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    /// <summary>Model (config): the rounds of the game. Answers must fit the 1-7 number buttons.</summary>
    [CreateAssetMenu(menuName = "Game/Otara/Level", fileName = "OtaraLevel")]
    public class OtaraLevelData : ScriptableObject
    {
        public const int MaxSheep = 7;

        [SerializeField] List<OtaraRound> rounds = new List<OtaraRound>();

        public IReadOnlyList<OtaraRound> Rounds => rounds;

        void OnValidate()
        {
            for (int i = 0; i < rounds.Count; i++)
            {
                var round = rounds[i];
                if (round.Answer < 1 || round.Answer > 7)
                    Debug.LogWarning($"{name}: round {i + 1} has answer {round.Answer}, but only the buttons 1-7 exist.", this);
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
