using System.Collections.Generic;
using IntellectualGame.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Manager of the currently running game. The main menu lists the games of the catalog and starts the chosen one;
    /// while a game is running the number buttons are forwarded to it as answers.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        enum State { Menu, Playing, Won }

        [SerializeField] GameCatalog catalog;
        [SerializeField] MainMenuView menuView;
        [SerializeField] Game[] games;
        [SerializeField] GameObject answerPad;
        [SerializeField] Button[] numberButtons;
        [SerializeField] Text hintText;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject winPanel;
        [SerializeField] Text winText;
        [SerializeField] Button replayButton;
        [SerializeField] Button menuButton;

        readonly Dictionary<int, Game> gamesByNumber = new Dictionary<int, Game>();
        State state;
        Game current;

        void Awake()
        {
            foreach (var game in games)
            {
                if (gamesByNumber.ContainsKey(game.MenuNumber))
                    Debug.LogWarning($"Menu number {game.MenuNumber} is used by both {gamesByNumber[game.MenuNumber].name} and {game.name}.", game);
                else
                    gamesByNumber.Add(game.MenuNumber, game);
            }

            for (int i = 0; i < numberButtons.Length; i++)
            {
                int number = i + 1;
                numberButtons[i].onClick.AddListener(() => OnNumberPressed(number));
            }

            menuView.GameSelected += OnGameSelected;
            quitButton.onClick.AddListener(ShowMenu);
            menuButton.onClick.AddListener(ShowMenu);
            replayButton.onClick.AddListener(Replay);
        }

        void OnDestroy()
        {
            if (menuView != null) menuView.GameSelected -= OnGameSelected;
        }

        void Start() => ShowMenu();

        void OnGameSelected(int number)
        {
            if (state == State.Menu && gamesByNumber.TryGetValue(number, out var game))
                Begin(game);
        }

        void OnNumberPressed(int number)
        {
            if (state == State.Playing) current.SubmitAnswer(number);
        }

        void Begin(Game game)
        {
            Release();
            current = game;
            current.Finished += OnGameFinished;

            state = State.Playing;
            menuView.Hide();
            winPanel.SetActive(false);
            answerPad.SetActive(true);
            quitButton.gameObject.SetActive(true);
            hintText.gameObject.SetActive(true);
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
            foreach (var game in gamesByNumber.Values) game.StopGame();

            state = State.Menu;
            winPanel.SetActive(false);
            answerPad.SetActive(false);
            quitButton.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
            menuView.Show(catalog, gamesByNumber.ContainsKey);
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
