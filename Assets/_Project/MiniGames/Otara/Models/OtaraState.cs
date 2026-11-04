using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Otara
{
    /// <summary>Model (runtime state): created once and reset for every run so the level asset is never modified.</summary>
    public class OtaraState : ScriptableObject
    {
        List<OtaraRound> rounds = new List<OtaraRound>();

        public int Index { get; private set; }
        public int Mistakes { get; private set; }
        public int Total => rounds.Count;
        public bool IsComplete => Index >= Total;
        public OtaraRound Current => rounds[Index];

        public void Begin(List<OtaraRound> run)
        {
            rounds = run;
            Index = 0;
            Mistakes = 0;
        }

        public void Advance() => Index++;
        public void RegisterMistake() => Mistakes++;
    }
}
