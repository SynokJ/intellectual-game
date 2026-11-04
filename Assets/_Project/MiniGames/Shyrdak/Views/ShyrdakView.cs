using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Shyrdak
{
    /// <summary>View: the sample carpet, the carpet being painted and the numbered palette. Knows no rules.</summary>
    public class ShyrdakView : MonoBehaviour
    {
        [SerializeField] GameObject stage;
        [SerializeField] GameObject paletteRoot;
        [SerializeField] RectTransform paletteContainer;
        [SerializeField] GameObject swatchTemplate;
        [SerializeField] SpriteRenderer[] sampleParts;
        [SerializeField] SpriteRenderer[] paintParts;
        [SerializeField] Transform paintCarpet;
        [SerializeField] Color blankColor = new Color(0.85f, 0.82f, 0.75f);

        readonly List<GameObject> swatches = new List<GameObject>();
        Tween highlight;

        public int PartCount => paintParts.Length;

        void Awake() => swatchTemplate.SetActive(false);

        public void ResetView()
        {
            highlight?.Kill();
            highlight = null;
            paintCarpet.DOKill();
            paintCarpet.localScale = Vector3.one;

            foreach (var part in paintParts)
            {
                part.transform.DOKill();
                part.transform.localScale = Vector3.one;
                part.color = blankColor;
            }
            foreach (var swatch in swatches) if (swatch != null) Destroy(swatch);
            swatches.Clear();
        }

        public void SetVisible(bool visible)
        {
            stage.SetActive(visible);
            paletteRoot.SetActive(visible);
        }

        /// <summary>Shows the numbered palette and colors the sample carpet.</summary>
        public void ShowSample(IReadOnlyList<Color> palette, IReadOnlyList<int> colors)
        {
            for (int i = 0; i < palette.Count; i++)
            {
                var swatch = Instantiate(swatchTemplate, paletteContainer);
                swatch.SetActive(true);
                swatch.GetComponent<Image>().color = palette[i];
                swatch.GetComponentInChildren<Text>().text = (i + 1).ToString();
                swatches.Add(swatch);
            }

            for (int i = 0; i < sampleParts.Length; i++)
                sampleParts[i].color = i < colors.Count ? palette[colors[i]] : blankColor;
        }

        /// <summary>Makes the part that has to be painted next pulse on both carpets.</summary>
        public void Highlight(int part)
        {
            highlight?.Kill();
            foreach (var renderer in paintParts) renderer.transform.localScale = Vector3.one;
            foreach (var renderer in sampleParts) renderer.transform.localScale = Vector3.one;

            var seq = DOTween.Sequence().SetLink(gameObject).SetLoops(-1, LoopType.Yoyo);
            seq.Append(paintParts[part].transform.DOScale(1.12f, 0.4f));
            seq.Join(sampleParts[part].transform.DOScale(1.12f, 0.4f));
            highlight = seq;
        }

        public IEnumerator PlayPaint(int part, Color color)
        {
            highlight?.Kill();
            paintParts[part].transform.localScale = Vector3.one;
            sampleParts[part].transform.localScale = Vector3.one;
            paintParts[part].color = color;
            yield return paintParts[part].transform.DOPunchScale(Vector3.one * 0.25f, 0.35f).SetLink(gameObject).WaitForCompletion();
        }

        public IEnumerator PlayFinale()
        {
            yield return paintCarpet.DOPunchScale(Vector3.one * 0.15f, 0.8f, 4).SetLink(gameObject).WaitForCompletion();
            yield return new WaitForSeconds(0.4f);
        }
    }
}
