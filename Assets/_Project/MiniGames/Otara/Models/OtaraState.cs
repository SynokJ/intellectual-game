using UnityEngine;

namespace IntellectualGame.Otara
{
    /// <summary>Model (runtime state): created once and reset for every run so the level asset is never modified.</summary>
    public class OtaraState : ScriptableObject
    {
        public int Index { get; private set; }
        public int Total { get; private set; }
        public int Mistakes { get; private set; }
        public bool IsComplete => Index >= Total;

        public void Begin(int total)
        {
            Total = total;
            Index = 0;
            Mistakes = 0;
        }

        public void Advance() => Index++;
        public void RegisterMistake() => Mistakes++;
    }
}
