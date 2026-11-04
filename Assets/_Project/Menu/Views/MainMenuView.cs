using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Menu
{
    /// <summary>View: the main menu with one card per game. Reports the chosen game, knows nothing about starting it.</summary>
    public class MainMenuView : MonoBehaviour
    {
        const float CardStagger = 0.08f;

        [SerializeField] GameObject root;
        [SerializeField] Text titleText;
        [SerializeField] Text subtitleText;
        [SerializeField] RectTransform cardContainer;
        [SerializeField] GameCardView cardTemplate;

        readonly List<GameCardView> cards = new List<GameCardView>();

        public event Action<int> GameSelected;

        void Awake() => cardTemplate.gameObject.SetActive(false);

        public void Show(GameCatalog catalog, Func<int, bool> isAvailable)
        {
            titleText.text = catalog.MenuTitle;
            subtitleText.text = catalog.MenuSubtitle;

            ClearCards();
            root.SetActive(true);
            for (int i = 0; i < catalog.Entries.Count; i++)
            {
                var entry = catalog.Entries[i];
                var card = Instantiate(cardTemplate, cardContainer);
                card.name = "Game Card " + entry.menuNumber;
                card.gameObject.SetActive(true);
                card.Bind(entry, isAvailable(entry.menuNumber), number => GameSelected?.Invoke(number));
                card.PlayAppear(i * CardStagger);
                cards.Add(card);
            }
        }

        public void Hide()
        {
            root.SetActive(false);
            ClearCards();
        }

        void ClearCards()
        {
            foreach (var card in cards)
                if (card != null) Destroy(card.gameObject);
            cards.Clear();
        }
    }
}
