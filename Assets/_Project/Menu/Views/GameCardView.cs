using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Menu
{
    /// <summary>View of one game card in the main menu.</summary>
    public class GameCardView : MonoBehaviour
    {
        const float LockedAlpha = 0.6f;

        [SerializeField] Image background;
        [SerializeField] Text numberText;
        [SerializeField] Text titleText;
        [SerializeField] Text descriptionText;
        [SerializeField] GameObject soonBadge;
        [SerializeField] Button button;
        [SerializeField] CanvasGroup group;

        float restAlpha = 1f;

        public void Bind(GameCatalogEntry entry, Action<GameCatalogEntry> onClick)
        {
            bool available = entry.IsAvailable;
            background.color = entry.accent;
            numberText.text = entry.menuNumber.ToString();
            titleText.text = entry.title;
            descriptionText.text = entry.description;
            soonBadge.SetActive(!available);

            button.interactable = available;
            restAlpha = available ? 1f : LockedAlpha;
            group.alpha = restAlpha;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick(entry));
        }

        public void PlayAppear(float delay)
        {
            group.alpha = 0f;
            transform.localScale = Vector3.one * 0.85f;
            DOTween.Sequence().SetLink(gameObject).SetDelay(delay)
                .Join(group.DOFade(restAlpha, 0.3f))
                .Join(transform.DOScale(1f, 0.35f).SetEase(Ease.OutBack));
        }
    }
}
