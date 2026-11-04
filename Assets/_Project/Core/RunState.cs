using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>Model (runtime state): the rounds of one run and the progress through them. Reset for every run.</summary>
    public abstract class RunState<T> : ScriptableObject
    {
        List<T> rounds = new List<T>();

        public int Index { get; private set; }
        public int Mistakes { get; private set; }
        public int Total => rounds.Count;
        public bool IsComplete => Index >= Total;
        public T Current => rounds[Index];

        public void Begin(List<T> run)
        {
            rounds = run;
            Index = 0;
            Mistakes = 0;
        }

        public void Advance() => Index++;
        public void RegisterMistake() => Mistakes++;
    }
}
