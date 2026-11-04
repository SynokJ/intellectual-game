using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

    /// <summary>Model (config): one equation per assembled part of the yurt.</summary>
    [CreateAssetMenu(menuName = "Game/Yurta/Level", fileName = "YurtaLevel")]
    public class YurtaLevelData : ScriptableObject
    {
        [SerializeField] List<YurtaEquation> equations = new List<YurtaEquation>();

        public IReadOnlyList<YurtaEquation> Equations => equations;

        /// <summary>The biggest answer of the level; the answer pad gets at least this many buttons.</summary>
        public int MaxAnswer => equations.Count == 0 ? 0 : equations.Max(equation => equation.Answer);

        void OnValidate()
        {
            foreach (var equation in equations)
                if (equation.Answer < 1)
                    Debug.LogWarning($"{name}: answer of '{equation.Text}' is {equation.Answer}, but the buttons start at 1.", this);
        }
    }
}
