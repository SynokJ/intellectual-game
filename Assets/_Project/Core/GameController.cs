using IntellectualGame.Menu;
using UnityEngine;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>
    /// Shell of the app: shows the main menu, spawns the prefab of the chosen game and destroys it when the player leaves.
    /// Each game prefab brings its own world, HUD and answer pad; the shell only owns the menu, hint, quit button and win panel.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        enum State { Menu, Playing, Won }

        [SerializeField] GameCatalog catalog;
        [SerializeField] MainMenuView menuView;
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

        void OnGameSelected(GameCatalogEntry entry)
        {
            if (state != State.Menu || !entry.IsAvailable) return;

            current = Instantiate(entry.prefab);
            current.name = entry.prefab.name;
            current.Finished += OnGameFinished;
            menuView.Hide();
            Play();
        }

        void Play()
        {
            state = State.Playing;
            winPanel.SetActive(false);
            quitButton.gameObject.SetActive(true);
            hintText.gameObject.SetActive(true);
            hintText.text = "Выбери правильный ответ";
            current.StartGame();
        }

        void Replay()
        {
            if (current != null) Play();
        }

        void ShowMenu()
        {
            DestroyCurrent();

            state = State.Menu;
            winPanel.SetActive(false);
            quitButton.gameObject.SetActive(false);
            hintText.gameObject.SetActive(false);
            menuView.Show(catalog);
        }

        void OnGameFinished(Game game, bool won)
        {
            state = State.Won;
            quitButton.gameObject.SetActive(false);
            winText.text = won ? $"Победа!\n{game.WinMessage}" : "Попробуй ещё раз";
            winPanel.SetActive(true);
        }

        void DestroyCurrent()
        {
            if (current == null) return;
            current.Finished -= OnGameFinished;
            current.StopGame();
            Destroy(current.gameObject);
            current = null;
        }
    }
}
