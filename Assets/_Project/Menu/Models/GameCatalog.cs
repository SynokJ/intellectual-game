using System;
using System.Collections.Generic;
using UnityEngine;

namespace IntellectualGame.Menu
{
    [Serializable]
    public struct GameCatalogEntry
    {
        [Tooltip("Matches Game.MenuNumber of the mini-game. An entry without a matching game is shown as 'coming soon'.")]
        public int menuNumber;
        public string title;
        [TextArea] public string description;
        public Color accent;
    }

    /// <summary>Model: what the main menu shows - its headings and one card per game.</summary>
    [CreateAssetMenu(menuName = "Game/Catalog", fileName = "GameCatalog")]
    public class GameCatalog : ScriptableObject
    {
        [SerializeField] string menuTitle = "Интеллектуальные игры";
        [SerializeField] string menuSubtitle = "Выбери игру";
        [SerializeField] List<GameCatalogEntry> entries = new List<GameCatalogEntry>();

        public string MenuTitle => menuTitle;
        public string MenuSubtitle => menuSubtitle;
        public IReadOnlyList<GameCatalogEntry> Entries => entries;
    }
}
