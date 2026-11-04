using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Menu
{
    /// <summary>
    /// View: the main menu with one tab per game category and one card per game of the chosen category.
    /// Reports the chosen game, knows nothing about starting it.
    /// </summary>
    public class MainMenuView : MonoBehaviour
    {
        const float CardStagger = 0.08f;

        [SerializeField] GameObject root;
        [SerializeField] Text titleText;
        [SerializeField] Text subtitleText;
        [SerializeField] RectTransform tabContainer;
        [SerializeField] CategoryTabView tabTemplate;
        [SerializeField] RectTransform cardContainer;
        [SerializeField] GameCardView cardTemplate;

        readonly List<CategoryTabView> tabs = new List<CategoryTabView>();
        readonly List<GameCardView> cards = new List<GameCardView>();
        GameCatalog catalog;
        int selectedCategory;

        public event Action<GameCatalogEntry> GameSelected;

        void Awake()
        {
            tabTemplate.gameObject.SetActive(false);
            cardTemplate.gameObject.SetActive(false);
        }

        public void Show(GameCatalog source)
        {
            catalog = source;
            titleText.text = catalog.MenuTitle;
            selectedCategory = Mathf.Clamp(selectedCategory, 0, Mathf.Max(0, catalog.Categories.Count - 1));

            root.SetActive(true);
            BuildTabs();
            ShowCategory(selectedCategory);
        }

        public void Hide()
        {
            root.SetActive(false);
            ClearTabs();
            ClearCards();
        }

        void BuildTabs()
        {
            ClearTabs();
            for (int i = 0; i < catalog.Categories.Count; i++)
            {
                int index = i;
                var tab = Instantiate(tabTemplate, tabContainer);
                tab.name = "Category Tab " + (i + 1);
                tab.gameObject.SetActive(true);
                tab.Bind(catalog.Categories[i], () => ShowCategory(index));
                tabs.Add(tab);
            }
        }

        void ShowCategory(int index)
        {
            selectedCategory = index;
            for (int i = 0; i < tabs.Count; i++) tabs[i].SetSelected(i == index);
            subtitleText.text = catalog.Categories.Count > 0 ? catalog.Categories[index].title : catalog.MenuSubtitle;

            ClearCards();
            int shown = 0;
            foreach (var entry in catalog.Entries)
            {
                if (entry.category != index) continue;

                var card = Instantiate(cardTemplate, cardContainer);
                card.name = "Game Card " + entry.menuNumber;
                card.gameObject.SetActive(true);
                card.Bind(entry, selected => GameSelected?.Invoke(selected));
                card.PlayAppear(shown++ * CardStagger);
                cards.Add(card);
            }
        }

        void ClearTabs()
        {
            foreach (var tab in tabs)
                if (tab != null) Destroy(tab.gameObject);
            tabs.Clear();
        }

        void ClearCards()
        {
            foreach (var card in cards)
                if (card != null) Destroy(card.gameObject);
            cards.Clear();
        }
    }
}
