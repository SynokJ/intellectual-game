using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Bazaar
{
    /// <summary>Model (runtime state): created once and reset for every run so the level asset is never modified.</summary>
    public class BazaarState : ScriptableObject
    {
        List<BazaarRound> rounds = new List<BazaarRound>();

        public int Index { get; private set; }
        public int Mistakes { get; private set; }
        public int Total => rounds.Count;
        public bool IsComplete => Index >= Total;
        public BazaarRound Current => rounds[Index];

        public void Begin(List<BazaarRound> run)
        {
            rounds = run;
            Index = 0;
            Mistakes = 0;
        }

        public void Advance() => Index++;
        public void RegisterMistake() => Mistakes++;
    }
}
