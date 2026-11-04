using System;
using System.Collections.Generic;
using System.Linq;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Kerege
{
    [Serializable]
    public struct KeregeWord
    {
        public string word;
        [Tooltip("Russian translation shown as the clue.")] public string hint;
    }

    /// <summary>One word of a run: the letters are already shuffled; tile number n (1-based) is tiles[n - 1].</summary>
    [Serializable]
    public struct KeregeRound
    {
        public string word;
        public string hint;
        public string tiles;
    }

    /// <summary>Model (config): the word bank. A random run takes a few words in random order with shuffled letter tiles.</summary>
    [CreateAssetMenu(menuName = "Game/Kerege/Level", fileName = "KeregeLevel")]
    public class KeregeLevelData : ScriptableObject
    {
        const int ShuffleAttempts = 8;

        [Tooltip("On: random words and tile order every run. Off: the whole bank in order, tiles in letter order.")]
        [SerializeField] bool randomize = true;
        [SerializeField, Min(1)] int wordCount = 4;
        [SerializeField] List<KeregeWord> words = new List<KeregeWord>();

        /// <summary>The longest word; the answer pad needs a button per tile.</summary>
        public int MaxAnswer => words.Count == 0 ? 0 : words.Max(entry => entry.word.Length);

        public List<KeregeRound> CreateRun()
        {
            var bank = new List<KeregeWord>(words);
            if (randomize) bank.Shuffle();

            int count = randomize ? Mathf.Min(wordCount, bank.Count) : bank.Count;
            var run = new List<KeregeRound>();
            for (int i = 0; i < count; i++)
                run.Add(new KeregeRound { word = bank[i].word, hint = bank[i].hint, tiles = randomize ? ShuffleLetters(bank[i].word) : bank[i].word });
            return run;
        }

        static string ShuffleLetters(string word)
        {
            var letters = word.ToCharArray();
            for (int attempt = 0; attempt < ShuffleAttempts; attempt++)
            {
                letters.Shuffle();
                if (new string(letters) != word) break;
            }
            return new string(letters);
        }
    }
}
