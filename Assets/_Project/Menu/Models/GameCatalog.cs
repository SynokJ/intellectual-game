using System;
using System.Collections.Generic;
using IntellectualGame.Core;
using UnityEngine;

namespace IntellectualGame.Menu
{
    [Serializable]
    public struct GameCategory
    {
        public string title;
        public Color accent;
    }

    [Serializable]
    public struct GameCatalogEntry
    {
        [Tooltip("Number shown on the card.")]
        public int menuNumber;
        [Tooltip("Index in the catalog's categories.")]
        public int category;
        public string title;
        [TextArea] public string description;
        public Color accent;
        [Tooltip("Self-contained game prefab (world, HUD, answer pad, controller). Empty = shown as 'coming soon'.")]
        public Game prefab;

        public bool IsAvailable => prefab != null;
    }

    /// <summary>Model: what the main menu shows - its headings, the game categories and one card per game, each pointing at its game prefab.</summary>
    [CreateAssetMenu(menuName = "Game/Catalog", fileName = "GameCatalog")]
    public class GameCatalog : ScriptableObject
    {
        [SerializeField] string menuTitle = "Интеллектуальные игры";
        [SerializeField] string menuSubtitle = "Выбери игру";
        [SerializeField] List<GameCategory> categories = new List<GameCategory>();
        [SerializeField] List<GameCatalogEntry> entries = new List<GameCatalogEntry>();

        public string MenuTitle => menuTitle;
        public string MenuSubtitle => menuSubtitle;
        public IReadOnlyList<GameCategory> Categories => categories;
        public IReadOnlyList<GameCatalogEntry> Entries => entries;
    }
}
