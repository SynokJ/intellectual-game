using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Core
{
    [Serializable]
    public struct QuizQuestion
    {
        [TextArea] public string text;
        public string correct;
        [Tooltip("Wrong answers; a run picks some of them at random.")] public string[] wrong;
        [Tooltip("Shown after a correct answer.")] [TextArea] public string note;
    }

    /// <summary>One question of a run: the options are already shuffled; correctNumber is 1-based like the answer pad.</summary>
    [Serializable]
    public struct QuizRound
    {
        public string text;
        public string[] options;
        public int correctNumber;
        public string note;
    }

    /// <summary>
    /// Model (config) of every multiple-choice game: its title, the question bank and how a run is drawn from it.
    /// A random run takes a few questions in random order, each with shuffled options.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Quiz/Level", fileName = "QuizLevel")]
    public class QuizLevelData : ScriptableObject
    {
        [SerializeField] string title = "Викторина";
        [SerializeField] string winMessage = "Все ответы верны!";
        [Tooltip("On: random questions and shuffled options every run. Off: the whole bank in order, correct answer first.")]
        [SerializeField] bool randomize = true;
        [SerializeField, Min(1)] int questionCount = 5;
        [SerializeField, Range(2, 9)] int optionCount = 4;
        [SerializeField] List<QuizQuestion> questions = new List<QuizQuestion>();

        public string Title => title;
        public string WinMessage => winMessage;
        public int MaxAnswer => optionCount;

        public List<QuizRound> CreateRun()
        {
            var bank = new List<QuizQuestion>(questions);
            if (randomize) bank.Shuffle();

            int count = randomize ? Mathf.Min(questionCount, bank.Count) : bank.Count;
            var run = new List<QuizRound>();
            for (int i = 0; i < count; i++) run.Add(BuildRound(bank[i]));
            return run;
        }

        QuizRound BuildRound(QuizQuestion question)
        {
            var wrong = new List<string>(question.wrong ?? new string[0]);
            if (randomize) wrong.Shuffle();
            if (wrong.Count > optionCount - 1) wrong.RemoveRange(optionCount - 1, wrong.Count - (optionCount - 1));

            var options = new List<string> { question.correct };
            options.AddRange(wrong);
            if (randomize) options.Shuffle();

            return new QuizRound
            {
                text = question.text,
                options = options.ToArray(),
                correctNumber = options.IndexOf(question.correct) + 1,
                note = question.note,
            };
        }

        void OnValidate()
        {
            foreach (var question in questions)
                if (string.IsNullOrEmpty(question.correct) || question.wrong == null || question.wrong.Length == 0)
                    Debug.LogWarning($"{name}: question '{question.text}' needs a correct answer and wrong answers.", this);
        }
    }
}
