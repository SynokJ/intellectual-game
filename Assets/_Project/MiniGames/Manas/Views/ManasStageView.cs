using System.Collections;
using DG.Tweening;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Manas
{
    /// <summary>View: the hero rides along the road toward the yurt; every right answer moves him one step on.</summary>
    public class ManasStageView : QuizStageView
    {
        [SerializeField] GameObject stage;
        [SerializeField] Transform hero;
        [SerializeField] Transform roadStart;
        [SerializeField] Transform roadEnd;
        [SerializeField] Transform finish;

        public override void ResetView()
        {
            hero.DOKill();
            finish.DOKill();
            hero.position = roadStart.position;
            hero.localScale = Vector3.one;
            finish.localScale = Vector3.one;
        }

        public override void SetVisible(bool visible) => stage.SetActive(visible);

        public override void Begin(int total) => ResetView();

        public override IEnumerator PlayCorrect(int index, int total)
        {
            Vector3 target = Vector3.Lerp(roadStart.position, roadEnd.position, (index + 1f) / total);
            hero.DOKill();
            yield return hero.DOJump(target, 0.8f, 2, 1f).SetLink(gameObject).WaitForCompletion();
        }

        public override IEnumerator PlayFinale()
        {
            yield return finish.DOPunchScale(Vector3.one * 0.3f, 0.8f, 4).SetLink(gameObject).WaitForCompletion();
            yield return hero.DOJump(hero.position, 1.2f, 3, 1.2f).SetLink(gameObject).WaitForCompletion();
        }
    }
}
