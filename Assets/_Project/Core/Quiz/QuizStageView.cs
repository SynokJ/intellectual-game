using System.Collections;
using UnityEngine;

namespace IntellectualGame.Core
{
    /// <summary>View of the world of a quiz game: what happens on the stage when answers are right. Each quiz game draws its own.</summary>
    public abstract class QuizStageView : MonoBehaviour
    {
        public abstract void ResetView();
        public abstract void SetVisible(bool visible);

        /// <summary>A run with <paramref name="total"/> questions starts.</summary>
        public abstract void Begin(int total);

        /// <summary>Question number <paramref name="index"/> (0-based) was answered correctly.</summary>
        public abstract IEnumerator PlayCorrect(int index, int total);

        /// <summary>All questions are answered.</summary>
        public abstract IEnumerator PlayFinale();
    }
}
