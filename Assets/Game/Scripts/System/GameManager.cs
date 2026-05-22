using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameManager: mengelola state game secara keseluruhan.
/// - MainMenu → Playing → Paused → GameEnd
/// Singleton yang persist antar scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    // ─── Singleton ──────────────────────────────────────────────────────────

    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── State ───────────────────────────────────────────────────────────────

    public enum GameState { MainMenu, Playing, Paused, GameEnd }

    private GameState currentState = GameState.MainMenu;
    public  GameState State        => currentState;

    public event System.Action<GameState> OnStateChanged;

    // ─── Scene Names ─────────────────────────────────────────────────────────

    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string gameScene     = "SampleScene";
    [SerializeField] private string endScene      = "EndScene";

    // ─── State Transitions ───────────────────────────────────────────────────

    public void StartGame()
    {
        SetState(GameState.Playing);
        SceneManager.LoadScene(gameScene);
        Time.timeScale = 1f;
    }

    public void StartGame(string sceneName)
    {
        if (!string.IsNullOrWhiteSpace(sceneName))
            gameScene = sceneName;

        StartGame();
    }

    public void StartGameInCurrentScene()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        if (currentState != GameState.Playing) return;
        SetState(GameState.Paused);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (currentState != GameState.Paused) return;
        SetState(GameState.Playing);
        Time.timeScale = 1f;
    }

    public void TriggerGameEnd()
    {
        SetState(GameState.GameEnd);
        Time.timeScale = 0f;
        GameHUD.Instance?.ShowEndScreen();

        if (DialogBox.Instance != null)
        {
            DialogBox.Instance.Show(
                "Petunjuk Sistem",
                "Semua misi selesai!\n\nTerima kasih sudah membantu warga kampung."
            );
        }
    }

    public void ReturnToMainMenu()
    {
        SetState(GameState.MainMenu);
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void RestartGame()
    {
        SetState(GameState.Playing);
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameScene);
    }

    private void SetState(GameState newState)
    {
        currentState = newState;
        OnStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State: {newState}");
    }

    // ─── Input: Pause Toggle ─────────────────────────────────────────────────

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.MainMenu && SceneManager.GetActiveScene().name != mainMenuScene)
            {
                StartGameInCurrentScene();
                PauseGame();
            }
            else if (currentState == GameState.Playing)  PauseGame();
            else if (currentState == GameState.Paused) ResumeGame();
        }
    }
}
