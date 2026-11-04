using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace IntellectualGame.Yurta
{
    public enum EquationOperator { Add, Subtract }

    [Serializable]
    public struct YurtaEquation
    {
        public int a;
        public EquationOperator op;
        public int b;

        public int Answer => op == EquationOperator.Add ? a + b : a - b;
        public string Text => $"{a} {(op == EquationOperator.Add ? "+" : "−")} {b} = ?";
    }

    /// <summary>
    /// Model (config): one equation per assembled part of the yurt. Either a fixed list, or new random equations
    /// generated from the limits below at the start of every run.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Yurta/Level", fileName = "YurtaLevel")]
    public class YurtaLevelData : ScriptableObject
    {
        const int GenerateAttempts = 30;

        [Tooltip("On: generate new random equations every run. Off: use the fixed list below.")]
        [SerializeField] bool randomize = true;
        [Tooltip("Random: how many equations (one per yurt part).")]
        [SerializeField, Min(1)] int equationCount = 5;
        [Tooltip("Random: the biggest answer, also the biggest number in an equation.")]
        [SerializeField, Range(3, 9)] int maxAnswer = 9;
        [Tooltip("Random: also generate subtraction.")]
        [SerializeField] bool allowSubtract = true;
        [SerializeField] List<YurtaEquation> equations = new List<YurtaEquation>();

        public IReadOnlyList<YurtaEquation> Equations => equations;

        /// <summary>The biggest answer the level can produce; the answer pad gets at least this many buttons.</summary>
        public int MaxAnswer => randomize ? maxAnswer : (equations.Count == 0 ? 0 : equations.Max(equation => equation.Answer));

        /// <summary>The equations of one run: random ones, or a copy of the fixed list.</summary>
        public List<YurtaEquation> CreateRun()
        {
            if (!randomize) return new List<YurtaEquation>(equations);

            var run = new List<YurtaEquation>();
            while (run.Count < equationCount)
            {
                var next = RandomEquation();
                for (int attempt = 0; attempt < GenerateAttempts && run.Contains(next); attempt++)
                    next = RandomEquation();
                run.Add(next);
            }
            return run;
        }

        YurtaEquation RandomEquation()
        {
            bool subtract = allowSubtract && Random.value < 0.4f;
            if (subtract)
            {
                int a = Random.Range(2, maxAnswer + 1);
                return new YurtaEquation { a = a, op = EquationOperator.Subtract, b = Random.Range(1, a) };
            }

            int first = Random.Range(1, maxAnswer);
            return new YurtaEquation { a = first, op = EquationOperator.Add, b = Random.Range(1, maxAnswer - first + 1) };
        }

        void OnValidate()
        {
            foreach (var equation in equations)
                if (equation.Answer < 1)
                    Debug.LogWarning($"{name}: answer of '{equation.Text}' is {equation.Answer}, but the buttons start at 1.", this);
        }
    }
}
