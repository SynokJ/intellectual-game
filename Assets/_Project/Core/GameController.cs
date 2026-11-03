using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Manager of the currently running game. In the menu a number button starts the game registered under that number;
    /// while a game is running the number buttons are forwarded to it as answers.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        enum State { Menu, Playing, Won }

        [SerializeField] Button[] numberButtons;
        [SerializeField] Text hintText;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject winPanel;
        [SerializeField] Text winText;
        [SerializeField] Button replayButton;
        [SerializeField] Button menuButton;

        readonly Dictionary<int, Game> games = new Dictionary<int, Game>();
        State state;
        Game current;

        void Awake()
        {
            foreach (var game in FindObjectsByType<Game>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (games.ContainsKey(game.MenuNumber))
                    Debug.LogWarning($"Menu number {game.MenuNumber} is used by both {games[game.MenuNumber].name} and {game.name}.", game);
                else
                    games.Add(game.MenuNumber, game);
            }

            for (int i = 0; i < numberButtons.Length; i++)
            {
                int number = i + 1;
                numberButtons[i].onClick.AddListener(() => OnNumberPressed(number));
            }

            quitButton.onClick.AddListener(ShowMenu);
            menuButton.onClick.AddListener(ShowMenu);
            replayButton.onClick.AddListener(Replay);
        }

        void Start() => ShowMenu();

        void OnNumberPressed(int number)
        {
            switch (state)
            {
                case State.Menu:
                    if (games.TryGetValue(number, out var game))
                        Begin(game);
                    else
                        hintText.text = "Эта игра скоро появится";
                    break;
                case State.Playing:
                    current.SubmitAnswer(number);
                    break;
            }
        }

        void Begin(Game game)
        {
            Release();
            current = game;
            current.Finished += OnGameFinished;

            state = State.Playing;
            winPanel.SetActive(false);
            quitButton.gameObject.SetActive(true);
            hintText.text = "Выбери правильный ответ";
            current.StartGame();
        }

        void Replay()
        {
            if (current != null) Begin(current);
        }

        void ShowMenu()
        {
            Release();
            foreach (var game in games.Values) game.StopGame();

            state = State.Menu;
            winPanel.SetActive(false);
            quitButton.gameObject.SetActive(false);

            var list = games.OrderBy(pair => pair.Key).Select(pair => $"{pair.Key} — {pair.Value.Title}");
            hintText.text = "Выбери игру:   " + string.Join("     ", list);
        }

        void OnGameFinished(Game game, bool won)
        {
            state = State.Won;
            quitButton.gameObject.SetActive(false);
            winText.text = won ? $"Победа!\n{game.WinMessage}" : "Попробуй ещё раз";
            winPanel.SetActive(true);
        }

        void Release()
        {
            if (current == null) return;
            current.Finished -= OnGameFinished;
        }
    }
}
