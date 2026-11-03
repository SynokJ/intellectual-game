using System;
using System.Collections.Generic;
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

    /// <summary>Model (config): one equation per assembled part of the yurt. Answers must fit the 1-7 number buttons.</summary>
    [CreateAssetMenu(menuName = "Game/Yurta/Level", fileName = "YurtaLevel")]
    public class YurtaLevelData : ScriptableObject
    {
        [SerializeField] List<YurtaEquation> equations = new List<YurtaEquation>();

        public IReadOnlyList<YurtaEquation> Equations => equations;

        void OnValidate()
        {
            foreach (var equation in equations)
                if (equation.Answer < 1 || equation.Answer > 7)
                    Debug.LogWarning($"{name}: answer of '{equation.Text}' is {equation.Answer}, but only the buttons 1-7 exist.", this);
        }
    }
}
