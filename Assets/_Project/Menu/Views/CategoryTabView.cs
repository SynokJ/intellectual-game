using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Menu
{
    /// <summary>View of one category tab in the main menu. Reports clicks, the menu decides what to show.</summary>
    public class CategoryTabView : MonoBehaviour
    {
        const float SelectedScale = 1.06f;
        static readonly Color RestTint = new Color(1f, 1f, 1f, 0.6f);

        [SerializeField] Image background;
        [SerializeField] Text label;
        [SerializeField] Button button;

        Color accent;

        public void Bind(GameCategory category, Action onClick)
        {
            accent = category.accent;
            label.text = category.title;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            background.color = selected ? accent : accent * RestTint;
            transform.DOKill();
            transform.DOScale(selected ? SelectedScale : 1f, 0.15f).SetLink(gameObject);
        }
    }
}
