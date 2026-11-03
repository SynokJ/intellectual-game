using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Manager of the currently running game. In the menu a number button starts the game with the same index;
    /// while a game is running the number buttons are forwarded to it as answers.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        enum State { Menu, Playing, Won }

        [SerializeField] Game[] games;
        [SerializeField] Button[] numberButtons;
        [SerializeField] Text hintText;
        [SerializeField] Button quitButton;
        [SerializeField] GameObject winPanel;
        [SerializeField] Text winText;
        [SerializeField] Button replayButton;
        [SerializeField] Button menuButton;

        State state;
        Game current;

        void Awake()
        {
            for (int i = 0; i < numberButtons.Length; i++)
            {
                int index = i;
                numberButtons[i].onClick.AddListener(() => OnNumberPressed(index));
            }

            quitButton.onClick.AddListener(ShowMenu);
            menuButton.onClick.AddListener(ShowMenu);
            replayButton.onClick.AddListener(Replay);
        }

        void Start() => ShowMenu();

        void OnNumberPressed(int index)
        {
            switch (state)
            {
                case State.Menu:
                    if (index < games.Length && games[index] != null)
                        Begin(games[index]);
                    else
                        hintText.text = "Эта игра скоро появится";
                    break;
                case State.Playing:
                    current.SubmitAnswer(index + 1);
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
            foreach (var game in games)
                if (game != null) game.StopGame();

            state = State.Menu;
            winPanel.SetActive(false);
            quitButton.gameObject.SetActive(false);
            hintText.text = "Нажми кнопку 1, чтобы начать игру «Юрта: сборка»";
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
