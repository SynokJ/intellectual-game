using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Yurta
{
    /// <summary>Model (runtime state): created fresh for every run so the level asset is never modified.</summary>
    public class YurtaState : ScriptableObject
    {
        List<YurtaEquation> equations = new List<YurtaEquation>();

        public int Index { get; private set; }
        public int Mistakes { get; private set; }
        public int Total => equations.Count;
        public bool IsComplete => Index >= Total;
        public YurtaEquation Current => equations[Index];

        public void Begin(List<YurtaEquation> run)
        {
            equations = run;
            Index = 0;
            Mistakes = 0;
        }

        public void Advance() => Index++;
        public void RegisterMistake() => Mistakes++;
    }
}
