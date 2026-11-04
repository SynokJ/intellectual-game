using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IntellectualGame.Shyrdak
{
    /// <summary>
    /// Model (config): the palette and the number of carpet parts. A random run gives every part a random palette color;
    /// the player repeats the sample by pressing the number of the right color.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Shyrdak/Level", fileName = "ShyrdakLevel")]
    public class ShyrdakLevelData : ScriptableObject
    {
        [Tooltip("On: random sample colors every run. Off: use the fixed colors below.")]
        [SerializeField] bool randomize = true;
        [SerializeField] Color[] palette =
        {
            new Color(0.78f, 0.16f, 0.16f), new Color(0.15f, 0.35f, 0.7f), new Color(0.95f, 0.77f, 0.2f),
            new Color(0.2f, 0.6f, 0.35f), new Color(0.95f, 0.95f, 0.9f), new Color(0.15f, 0.12f, 0.12f),
        };
        [Tooltip("How many parts the carpet has; must match the carpet in the game prefab.")]
        [SerializeField, Min(1)] int partCount = 8;
        [Tooltip("Palette indexes of the parts when randomize is off.")]
        [SerializeField] List<int> fixedColors = new List<int>();

        public IReadOnlyList<Color> Palette => palette;
        public int MaxAnswer => palette.Length;

        /// <summary>Palette index of every carpet part in one run.</summary>
        public List<int> CreateRun()
        {
            if (!randomize) return new List<int>(fixedColors);

            var run = new List<int>();
            for (int i = 0; i < partCount; i++)
            {
                // never the same color twice in a row, so neighbouring parts stay distinguishable
                int color;
                do color = Random.Range(0, palette.Length);
                while (run.Count > 0 && color == run[run.Count - 1] && palette.Length > 1);
                run.Add(color);
            }
            return run;
        }
    }
}
