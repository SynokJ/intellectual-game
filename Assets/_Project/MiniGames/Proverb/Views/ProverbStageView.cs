using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Proverb
{
    /// <summary>View: a felt banner with one ornament gem per proverb; every solved proverb lights its gem.</summary>
    public class ProverbStageView : QuizStageView
    {
        [SerializeField] GameObject stage;
        [SerializeField] SpriteRenderer gemTemplate;
        [SerializeField] Transform gemAnchor;
        [SerializeField] float gemSpacing = 1.4f;
        [SerializeField] Color emptyColor = new Color(0.8f, 0.75f, 0.65f);
        [SerializeField] Color litColor = new Color(0.95f, 0.77f, 0.2f);

        readonly List<SpriteRenderer> gems = new List<SpriteRenderer>();

        void Awake() => gemTemplate.gameObject.SetActive(false);

        public override void ResetView()
        {
            foreach (var gem in gems)
                if (gem != null) Destroy(gem.gameObject);
            gems.Clear();
        }

        public override void SetVisible(bool visible) => stage.SetActive(visible);

        public override void Begin(int total)
        {
            ResetView();
            for (int i = 0; i < total; i++)
            {
                var gem = Instantiate(gemTemplate, gemTemplate.transform.parent);
                gem.gameObject.SetActive(true);
                gem.color = emptyColor;
                gem.transform.position = gemAnchor.position + Vector3.right * ((i - (total - 1) / 2f) * gemSpacing);
                gems.Add(gem);
            }
        }

        public override IEnumerator PlayCorrect(int index, int total)
        {
            var gem = gems[index];
            gem.color = litColor;
            yield return gem.transform.DOPunchScale(Vector3.one * 0.6f, 0.6f, 5).SetLink(gem.gameObject).WaitForCompletion();
        }

        public override IEnumerator PlayFinale()
        {
            var sequence = DOTween.Sequence().SetLink(gameObject);
            for (int i = 0; i < gems.Count; i++)
                sequence.Join(gems[i].transform.DOPunchPosition(Vector3.up * 0.6f, 0.6f, 1, 0f).SetDelay(0.1f * i));
            sequence.AppendInterval(0.3f);
            yield return sequence.WaitForCompletion();
        }
    }
}
