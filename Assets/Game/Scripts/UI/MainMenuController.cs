using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject howToPanel;

    [Header("Objects Hidden While Menu Is Open")]
    [SerializeField] private GameObject[] hideWhileMenuOpen;

    [Header("Game Flow")]
    [SerializeField] private string gameSceneName = "SampleScene";
    [SerializeField] private bool loadGameSceneOnStart = true;
    [SerializeField] private bool freezeGameplayWhileMenuOpen = true;

    [Header("Selection")]
    [SerializeField] private GameObject firstSelectedButton;

    private bool hasStartedGame;

    private void Start()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        hasStartedGame = false;
        SetMenuVisible(true);
        SetHowToVisible(false);
        SetHiddenObjectsVisible(false);

        if (freezeGameplayWhileMenuOpen)
            Time.timeScale = 0f;

        SelectFirstButton();
    }

    public void StartGame()
    {
        if (hasStartedGame) return;

        hasStartedGame = true;
        SetHowToVisible(false);
        SetMenuVisible(false);
        SetHiddenObjectsVisible(true);
        Time.timeScale = 1f;

        if (loadGameSceneOnStart)
        {
            GameManager manager = GetOrCreateGameManager();
            if (manager != null)
                manager.StartGame(gameSceneName);
            else if (!string.IsNullOrWhiteSpace(gameSceneName))
                SceneManager.LoadScene(gameSceneName);

            return;
        }

        if (GameManager.Instance != null)
            GameManager.Instance.StartGameInCurrentScene();
    }

    public void ShowHowTo()
    {
        SetHowToVisible(true);
    }

    public void HideHowTo()
    {
        SetHowToVisible(false);
        SelectFirstButton();
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private GameManager GetOrCreateGameManager()
    {
        if (GameManager.Instance != null)
            return GameManager.Instance;

        GameObject managerObject = new GameObject("GameManager");
        return managerObject.AddComponent<GameManager>();
    }

    private void SetMenuVisible(bool visible)
    {
        if (menuRoot != null)
            menuRoot.SetActive(visible);
    }

    private void SetHowToVisible(bool visible)
    {
        if (howToPanel != null)
            howToPanel.SetActive(visible);
    }

    private void SetHiddenObjectsVisible(bool visible)
    {
        if (hideWhileMenuOpen == null) return;

        foreach (GameObject item in hideWhileMenuOpen)
        {
            if (item != null)
                item.SetActive(visible);
        }
    }

    private void SelectFirstButton()
    {
        if (firstSelectedButton == null || EventSystem.current == null) return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
}
